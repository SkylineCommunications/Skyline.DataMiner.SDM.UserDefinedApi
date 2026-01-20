namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	using SLDataGateway.API.Querying;
	using SLDataGateway.API.Types.Querying;

	public class ODataSdmTranslator<T> : ODataVisitor
		where T : class
	{
		private readonly string _basePath = String.Empty;
		private readonly ODataToFilterElementConverter _converter;
		////private readonly Func<MemberInfo, Comparer, object, FilterElement<T>> _createFilter;
		////private readonly Func<MemberInfo, SortOrder, bool, IOrderByElement> _createOrder;

		private FilterElement<T> _filter = new TRUEFilterElement<T>();
		private IQuery<T> _query = new TRUEFilterElement<T>().ToQuery();

		public ODataSdmTranslator()
		{
			_converter = new ODataToFilterElementConverter<T>();
		}

		// Is used when context changes happen,
		// This can be when we go 1 level deeper in a collection (any)
		// This can be when we go 1 property level deeper MyProp.MyType.Name
		internal ODataSdmTranslator(ODataToFilterElementConverter converter, string basePath)
		{
			_converter = converter;
			_basePath = basePath;
		}

		public FilterElement<T> TranslateFilter(string filter)
		{
			var tree = new ODataFilterParser(filter).Parse();
			Visit(tree);
			return _filter;
		}

		public IQuery<T> TranslateOrderBy(string orderBy)
		{
			var tree = new ODataOrderByParser(orderBy).Parse();
			Visit(tree);
			return _query;
		}

		public IQuery<T> Translate(string filter, string orderBy)
		{
			// Translate the filter
			var filterTree = new ODataFilterParser(filter).Parse();
			Visit(filterTree);
			_query = _filter.ToQuery();

			// Translate the sort order
			var orderTree = new ODataOrderByParser(orderBy).Parse();
			Visit(orderTree);
			return _query;
		}

		public override void VisitFilter(FilterNode node)
		{
			// Start of all filters
			_filter = new TRUEFilterElement<T>();

			Visit(node.Filter);
		}

		public override void VisitAnd(AndNode node)
		{
			var andFilters = new List<FilterElement<T>>();

			foreach (var child in node.Nodes)
			{
				Visit(child);
				andFilters.Add(_filter);
			}

			_filter = new ANDFilterElement<T>(andFilters.ToArray());
		}

		public override void VisitOr(OrNode node)
		{
			var orFilters = new List<FilterElement<T>>();

			foreach (var child in node.Nodes)
			{
				Visit(child);
				orFilters.Add(_filter);
			}

			_filter = new ORFilterElement<T>(orFilters.ToArray());
		}

		public override void VisitNot(NotNode node)
		{
			Visit(node.Child);
			_filter = _filter.NOT();
		}

		public override void VisitFunctionCall(FunctionCallNode node)
		{
			switch (node.Name)
			{
				case "contains":
				{
					if (!_converter.TryGetMember(node.Arguments[0], out var memberInfo) ||
						!_converter.TryGetLiteral(node.Arguments[1], out var literal))
					{
						throw new NotSupportedException($"Unsupported comparison: {node}");
					}

					_filter = CreateFilter(GetPathForMember(memberInfo!), Comparer.Contains, _converter.ParseLiteral(memberInfo!, literal!));
					break;
				}

				default:
				{
					throw new NotSupportedException($"Function '{node.Name}' is not supported.");
				}
			}
		}

		public override void VisitMethodCall(MethodCallNode node)
		{
			switch (node.Name)
			{
				case "any":
				{
					if (!_converter.TryGetMember(node.Target, out var memberInfo) || !(memberInfo is PropertyInfo pi))
					{
						throw new NotSupportedException($"Unsupported comparison: {node}");
					}

					var translator = new ODataSdmTranslator<T>(
						//new ODataToFilterElementConverter(pi!.PropertyType, node.Variable.Path),
						new ODataToFilterElementConverter(pi!.PropertyType),
						GetPathForMember(memberInfo));

					translator.Visit(node.Body);
					_filter = translator._filter;

					break;
				}

				default:
				{
					throw new NotSupportedException($"Method '{node.Name}' is not supported.");
				}
			}
		}

		public override void VisitBinary(BinaryNode node)
		{
			if (!_converter.TryGetOperand(node, out var comparer))
			{
				throw new NotSupportedException($"Unsupported comparison: {node}");
			}

			if (!((_converter.TryGetMember(node.Left, out var memberInfo) && _converter.TryGetLiteral(node.Right, out var literal)) ||
				(_converter.TryGetMember(node.Right, out memberInfo) && _converter.TryGetLiteral(node.Left, out literal))))
			{
				throw new NotSupportedException($"Unsupported comparison: {node}");
			}

			_filter = CreateFilter(GetPathForMember(memberInfo!), comparer, _converter.ParseLiteral(memberInfo!, literal!));
		}

		public override void VisitOrder(OrderNode node)
		{
			if (node.Order == ODataOrder.None)
			{
				// No order specified, skip.
				return;
			}

			if (!_converter.TryGetMember(node.Property, out var memberInfo))
			{
				throw new NotSupportedException($"Unsupported property to order by: '{node.Property.Path}'");
			}

			if (!_converter.TryGetOrder(node, out var sortOrder))
			{
				throw new NotSupportedException($"Unsupported order type: '{node.Order}'");
			}

			if (!_query.Order.Elements.Any())
			{
				_query = _query.WithOrder(
					OrderBy.Default.SingleConcat(CreateOrderBy(GetPathForMember(memberInfo!), sortOrder, false)));
			}
			else
			{
				_query = _query.WithOrder(
					_query.Order.SingleConcat(CreateOrderBy(GetPathForMember(memberInfo!), sortOrder, false)));
			}
		}

		private static FilterElement<T> CreateFilter(string path, Comparer comparer, object value)
		{
			var exposer = ExposerRegistry.findExposer(typeof(T), path);
			if (exposer is null)
			{
				throw new NotSupportedException($"Could not find an exposer for property with path '{path}'");
			}

			return FilterElementFactory.Create<T>(exposer, comparer, value);
		}

		private static IOrderByElement CreateOrderBy(string path, SortOrder sortOrder, bool naturalSort = false)
		{
			var exposer = ExposerRegistry.findExposer(typeof(T), path);
			if (exposer is null)
			{
				throw new NotSupportedException($"Could not find an exposer for property with path '{path}'");
			}

			return OrderByElementFactory.Create(exposer, sortOrder, naturalSort);
		}

		private string GetPathForMember(MemberInfo memberInfo)
		{
			if (String.IsNullOrEmpty(_basePath))
			{
				return memberInfo.Name;
			}

			return String.Join(".", _basePath, memberInfo.Name);
		}
	}
}
