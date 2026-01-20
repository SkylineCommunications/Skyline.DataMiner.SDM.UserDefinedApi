namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	using System;

	public class BinaryNode : ODataNode<BinaryNode>
	{
		internal BinaryNode(ODataNode left, ODataLogicalOperation operation, ODataNode right, ODataNode? parent = null)
			: base(parent)
		{
			Left = left?.WithParentInternal(this) ?? throw new ArgumentNullException(nameof(left));
			Operation = operation;
			Right = right?.WithParentInternal(this) ?? throw new ArgumentNullException(nameof(right));
		}

		public ODataLogicalOperation Operation { get; }

		public ODataNode Left { get; }

		public ODataNode Right { get; }

		public BinaryNode Update(
			ODataNode left,
			ODataLogicalOperation operation,
			ODataNode right,
			ODataNode? parent)
		{
			if (left != Left || operation != Operation || right != Right || parent != Parent)
			{
				var newNode = new BinaryNode(left, operation, right, parent);
				return newNode;
			}

			return this;
		}

		public override void Accept(IODataVisitor visitor)
		{
			visitor.VisitBinary(this);
		}

		public BinaryNode WithLeft(ODataNode left)
		{
			return Update(left, Operation, Right, Parent);
		}

		public BinaryNode WithOperation(ODataLogicalOperation operation)
		{
			return Update(Left, operation, Right, Parent);
		}

		public BinaryNode WithRight(ODataNode right)
		{
			return Update(Left, Operation, right, Parent);
		}

		protected internal override ODataNode WithParentInternal(ODataNode? parent)
		{
			return Update(Left, Operation, Right, parent);
		}
	}
}
