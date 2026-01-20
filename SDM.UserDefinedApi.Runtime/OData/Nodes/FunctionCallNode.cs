namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;

	public class FunctionCallNode : ODataNode<FunctionCallNode>
	{
		public FunctionCallNode(string methodName, IEnumerable<ODataNode> arguments, ODataNode? parent = null)
			: base(parent)
		{
			Name = methodName ?? throw new ArgumentNullException(nameof(methodName));

			if (arguments is null)
			{
				throw new ArgumentNullException(nameof(arguments));
			}

			var list = new List<ODataNode>();
			foreach (var argument in arguments)
			{
				list.Add(argument.WithParentInternal(this));
			}

			Arguments = new ReadOnlyCollection<ODataNode>(list);
		}

		public string Name { get; }

		public IReadOnlyList<ODataNode> Arguments { get; }

		public FunctionCallNode Update(
			string methodName,
			IEnumerable<ODataNode> arguments,
			ODataNode? parent)
		{
			if (Name != methodName ||
				Arguments.SequenceEqual(arguments) ||
				Parent != parent)
			{
				var newNode = new FunctionCallNode(methodName, arguments, parent);
				return newNode;
			}

			return this;
		}

		public FunctionCallNode WithName(string methodName)
		{
			return Update(methodName, Arguments, Parent);
		}

		public FunctionCallNode WithArgument(IEnumerable<ODataNode> arguments)
		{
			return Update(Name, arguments, Parent);
		}

		public FunctionCallNode AddArgument(ODataNode argument)
		{
			var arguments = Arguments.ToList();
			arguments.Add(argument);
			return Update(Name, arguments, Parent);
		}

		public override void Accept(IODataVisitor visitor)
		{
			visitor.VisitFunctionCall(this);
		}

		protected internal override ODataNode WithParentInternal(ODataNode? parent)
		{
			return Update(Name, Arguments, parent);
		}
	}
}
