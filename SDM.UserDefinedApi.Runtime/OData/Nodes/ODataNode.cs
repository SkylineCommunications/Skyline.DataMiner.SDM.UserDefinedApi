namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	public interface IODataNode
	{
		ODataNode? Parent { get; }

		void Accept(IODataVisitor visitor);
	}

	public abstract class ODataNode : IODataNode
	{
		protected ODataNode(ODataNode? parent = null)
		{
			Parent = parent;
		}

		public ODataNode? Parent { get; }

		public abstract void Accept(IODataVisitor visitor);

		protected internal abstract ODataNode WithParentInternal(ODataNode? parent);
	}

	public abstract class ODataNode<TNode> : ODataNode
		where TNode : ODataNode
	{
		protected ODataNode(ODataNode? parent = null)
			: base(parent)
		{
		}

		public TNode WithParent(ODataNode? parent)
		{
			return (TNode)WithParentInternal(parent);
		}
	}
}
