namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Reflection;

	internal class ODataToFilterElementConverter
	{
		private readonly Dictionary<string, MemberInfo> _properties = new Dictionary<string, MemberInfo>();

		public ODataToFilterElementConverter(Type type)
		{
			var actualType = type;
			if (TryGetCollectionElementType(type, out var elementType))
			{
				actualType = elementType;
			}

			_properties = actualType
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.ToDictionary(p => p.Name, p => (MemberInfo)p);
		}

		public ODataToFilterElementConverter(Dictionary<string, MemberInfo> properties)
		{
			_properties = properties ?? throw new ArgumentNullException(nameof(properties));
		}

		public IReadOnlyDictionary<string, MemberInfo> Properties { get => new ReadOnlyDictionary<string, MemberInfo>(_properties); }

		public object ParseLiteral(MemberInfo memberInfo, LiteralNode literal)
		{
			if (memberInfo is not PropertyInfo pi)
			{
				throw new NotSupportedException($"Unsupported member type: {memberInfo.GetType().FullName}");
			}

			var propertyType = pi.PropertyType;
			var value = ParseLiteral(propertyType, literal);
			return value;
		}

		public bool TryGetMember(ODataNode? node, out MemberInfo? memberInfo)
		{
			memberInfo = null;

			if (node is null)
			{
				return false;
			}

			if (!(node is PropertyNode pn))
			{
				return false;
			}

			// Happens in case this is a MethodCallNode
			// Colection/any(e: e/Property eq 'Value')
			var path = pn.Path;
			var indexOfSlash = pn.Path.IndexOf('/');
			if (indexOfSlash > 0)
			{
				path = pn.Path.Substring(indexOfSlash + 1);
			}

			if (_properties.TryGetValue(path, out memberInfo))
			{
				return true;
			}

			return false;
		}

		public bool TryGetLiteral(ODataNode? node, out LiteralNode? value)
		{
			value = null;

			if (node is null)
			{
				return false;
			}

			if (node is LiteralNode ln)
			{
				value = ln;
				return true;
			}

			return false;
		}

		public bool TryGetOperand(BinaryNode? node, out Skyline.DataMiner.Net.Messages.SLDataGateway.Comparer comparer)
		{
			comparer = Net.Messages.SLDataGateway.Comparer.Equals;

			switch (node?.Operation)
			{
				case ODataLogicalOperation.Equal:
					comparer = Net.Messages.SLDataGateway.Comparer.Equals;
					break;

				case ODataLogicalOperation.NotEqual:
					comparer = Net.Messages.SLDataGateway.Comparer.NotEquals;
					break;

				case ODataLogicalOperation.GreaterThan when node?.Left is PropertyNode:
				case ODataLogicalOperation.LessThan when node?.Right is PropertyNode:
					comparer = Net.Messages.SLDataGateway.Comparer.GT;
					break;

				case ODataLogicalOperation.GreaterThanOrEqual when node?.Left is PropertyNode:
				case ODataLogicalOperation.LessThanOrEqual when node?.Right is PropertyNode:
					comparer = Net.Messages.SLDataGateway.Comparer.GTE;
					break;

				case ODataLogicalOperation.LessThan when node?.Left is PropertyNode:
				case ODataLogicalOperation.GreaterThan when node?.Right is PropertyNode:
					comparer = Net.Messages.SLDataGateway.Comparer.LT;
					break;

				case ODataLogicalOperation.LessThanOrEqual when node?.Left is PropertyNode:
				case ODataLogicalOperation.GreaterThanOrEqual when node?.Right is PropertyNode:
					comparer = Net.Messages.SLDataGateway.Comparer.LTE;
					break;

				default:
					return false;
			}

			return true;
		}

		public bool TryGetOrder(OrderNode? node, out SLDataGateway.API.Types.Querying.SortOrder sortOrder)
		{
			switch (node?.Order)
			{
				case ODataOrder.Ascending:
					sortOrder = SLDataGateway.API.Types.Querying.SortOrder.Ascending;
					return true;

				case ODataOrder.Descending:
					sortOrder = SLDataGateway.API.Types.Querying.SortOrder.Descending;
					return true;

				default:
					sortOrder = SLDataGateway.API.Types.Querying.SortOrder.None;
					return false;
			}
		}

		private static bool IsBasicType(Type type)
		{
			if (type.IsPrimitive || type.IsEnum)
				return true;

			if (type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(Guid) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan))
				return true;

			// Nullable<T> where T is a basic type
			if (Nullable.GetUnderlyingType(type) is Type underlying)
				return IsBasicType(underlying);

			return false;
		}

		private static bool TryGetCollectionElementType(Type type, out Type elementType)
		{
			// Handle arrays
			if (type.IsArray)
			{
				elementType = type.GetElementType()!;
				return true;
			}

			// Handle generic collections (List<T>, IEnumerable<T>, ICollection<T>, etc.)
			if (type.IsGenericType)
			{
				var genDef = type.GetGenericTypeDefinition();
				if (typeof(IEnumerable<>).IsAssignableFrom(genDef) ||
					typeof(ICollection<>).IsAssignableFrom(genDef) ||
					typeof(IList<>).IsAssignableFrom(genDef) ||
					typeof(List<>).IsAssignableFrom(genDef))
				{
					elementType = type.GetGenericArguments()[0];
					return true;
				}
			}

			// Handle types that *implement* IEnumerable<T> somewhere
			var enumerableInterface = type.GetInterfaces()
				.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

			if (enumerableInterface != null)
			{
				elementType = enumerableInterface.GetGenericArguments()[0];
				return true;
			}

			elementType = null!;
			return false;
		}

		private static object ParseLiteral(Type type, LiteralNode literal)
		{
			TryGetCollectionElementType(type, out var elementType);

			if (type == typeof(string) ||
				elementType == typeof(string))
			{
				return Convert.ToString(literal.Value);
			}
			else if (type == typeof(bool) ||
				elementType == typeof(bool))
			{
				return Convert.ToBoolean(literal.Value);
			}
			else if (type == typeof(byte) ||
				elementType == typeof(byte))
			{
				return Convert.ToByte(literal.Value);
			}
			else if (type == typeof(sbyte) ||
				elementType == typeof(sbyte))
			{
				return Convert.ToSByte(literal.Value);
			}
			else if (type == typeof(short) ||
				elementType == typeof(short))
			{
				return Convert.ToInt16(literal.Value);
			}
			else if (type == typeof(ushort) ||
				elementType == typeof(ushort))
			{
				return Convert.ToUInt16(literal.Value);
			}
			else if (type == typeof(int) ||
				elementType == typeof(int))
			{
				return Convert.ToInt32(literal.Value);
			}
			else if (type == typeof(uint) ||
				elementType == typeof(uint))
			{
				return Convert.ToUInt32(literal.Value);
			}
			else if (type == typeof(long) ||
				elementType == typeof(long))
			{
				return Convert.ToInt64(literal.Value);
			}
			else if (type == typeof(ulong) ||
				elementType == typeof(ulong))
			{
				return Convert.ToUInt64(literal.Value);
			}
			else if (type == typeof(DateTime) ||
				elementType == typeof(DateTime))
			{
				return DateTime.Parse(Convert.ToString(literal.Value));
			}
			else if (type == typeof(TimeSpan) ||
				elementType == typeof(TimeSpan))
			{
				return TimeSpan.Parse(Convert.ToString(literal.Value));
			}
			else if (type == typeof(double) ||
				elementType == typeof(double))
			{
				return Convert.ToDouble(literal.Value);
			}
			else if (type == typeof(float) ||
				elementType == typeof(float))
			{
				return Convert.ToSingle(literal.Value);
			}
			else if (type == typeof(decimal) ||
				elementType == typeof(decimal))
			{
				return Convert.ToDecimal(literal.Value);
			}
			else if (type == typeof(Guid) ||
				elementType == typeof(Guid))
			{
				return Guid.Parse(Convert.ToString(literal.Value));
			}
			else if (type.IsEnum ||
				(elementType?.IsEnum ?? false))
			{
				return Enum.Parse(type, Convert.ToString(literal.Value));
			}
			else if ((type.IsGenericType && type.GetGenericTypeDefinition().FullName == "Skyline.DataMiner.SDM.SdmObjectReference`1") ||
				((elementType?.IsGenericType ?? false) && elementType?.GetGenericTypeDefinition().FullName == "Skyline.DataMiner.SDM.SdmObjectReference`1"))
			{
				// Handle SdmObjectReference<T>
				var guid = Guid.Parse(Convert.ToString(literal.Value));
				var constructor = type.GetConstructor(new Type[] { typeof(Guid) });
				if (constructor == null)
				{
					throw new NotSupportedException($"No suitable constructor found for type: {type.FullName}");
				}

				return constructor.Invoke(new object[] { guid });
			}
			else if (Nullable.GetUnderlyingType(type) is Type underlying)
			{
				// Handle nullable types
				if (string.Equals(literal.Value?.ToString(), "null", StringComparison.OrdinalIgnoreCase))
				{
					return null;
				}
				else
				{
					return ParseLiteral(underlying, literal);
				}
			}
			else
			{
				throw new NotSupportedException($"Unsupported property type: {type.FullName}");
			}
		}
	}

	internal class ODataToFilterElementConverter<T>
		: ODataToFilterElementConverter
	{
		public ODataToFilterElementConverter()
			: base(typeof(T))
		{
		}
	}
}
