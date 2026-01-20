namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	using System;

	public class NotNode : ODataNode<NotNode>
	{
		internal NotNode(ODataNode child, ODataNode? parent = null)
			: base(parent)
		{
			Child = child?.WithParentInternal(this) ?? throw new ArgumentNullException(nameof(child));
		}

		public ODataLogicalOperation Operation { get; } = ODataLogicalOperation.Not;

		public ODataNode Child { get; }

		public NotNode Update(
			ODataNode child,
			ODataNode? parent)
		{
			if (child != Child || parent != Parent)
			{
				var newNode = new NotNode(child, parent);
				return newNode;
			}

			return this;
		}

		public override void Accept(IODataVisitor visitor)
		{
			visitor.VisitNot(this);
		}

		public NotNode WithChild(ODataNode child)
		{
			return Update(child, Parent);
		}

		protected internal override ODataNode WithParentInternal(ODataNode? parent)
		{
			return Update(Child, parent);
		}
	}
}
