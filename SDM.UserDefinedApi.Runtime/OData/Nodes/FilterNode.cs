namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	public class FilterNode : ODataNode<FilterNode>
	{
		public static readonly FilterNode Empty = new FilterNode(null);

		internal FilterNode(ODataNode? filter = null)
			: base(null)
		{
			Filter = filter?.WithParentInternal(this);
		}

		public ODataNode? Filter { get; }

		public FilterNode Update(
			ODataNode? filter)
		{
			if(filter != Filter)
			{
				var newNode = new FilterNode(filter);
				return newNode;
			}

			return this;
		}

		public override void Accept(IODataVisitor visitor)
		{
			visitor.VisitFilter(this);
		}

		public FilterNode WithFilter(ODataNode? filter)
		{
			return Update(filter);
		}

		protected internal override ODataNode WithParentInternal(ODataNode? parent)
		{
			// Filter nodes cannot have a parent they are top level nodes
			return this;
		}
	}
}
