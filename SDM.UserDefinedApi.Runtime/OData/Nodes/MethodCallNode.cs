namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	using System;

	public class MethodCallNode : ODataNode<MethodCallNode>
	{
		public MethodCallNode(string methodName, PropertyNode target, PropertyNode variable, ODataNode body, ODataNode? parent = null)
			: base(parent)
		{
			Name = methodName ?? throw new ArgumentNullException(nameof(methodName));
			Target = target ?? throw new ArgumentNullException(nameof(target));
			Variable = variable ?? throw new ArgumentNullException(nameof(variable));
			Body = body?.WithParentInternal(this) ?? throw new ArgumentNullException(nameof(body));
		}

		public string Name { get; }

		public PropertyNode Target { get; }

		public PropertyNode Variable { get; }

		public ODataNode Body { get; }

		public MethodCallNode Update(
			string methodName,
			PropertyNode target,
			PropertyNode variable,
			ODataNode body,
			ODataNode? parent)
		{
			if (Name != methodName ||
				Target != target ||
				Variable != variable ||
				Body != body ||
				Parent != parent)
			{
				var newNode = new MethodCallNode(methodName, target, variable, body, parent);
				return newNode;
			}

			return this;
		}

		public MethodCallNode WithName(string methodName)
		{
			return Update(methodName, Target, Variable, Body, Parent);
		}

		public MethodCallNode WithTarget(PropertyNode target)
		{
			return Update(Name, target, Variable, Body, Parent);
		}

		public MethodCallNode WithVariable(PropertyNode variable)
		{
			return Update(Name, Target, variable, Body, Parent);
		}

		public MethodCallNode WithBody(ODataNode body)
		{
			return Update(Name, Target, Variable, body, Parent);
		}

		public override void Accept(IODataVisitor visitor)
		{
			visitor.VisitMethodCall(this);
		}

		protected internal override ODataNode WithParentInternal(ODataNode? parent)
		{
			return Update(Name, Target, Variable, Body, parent);
		}
	}
}
