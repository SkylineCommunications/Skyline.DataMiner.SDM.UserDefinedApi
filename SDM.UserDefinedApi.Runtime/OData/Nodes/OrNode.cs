namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;

	public class OrNode : ODataNode<OrNode>
	{
		internal OrNode(IEnumerable<ODataNode> nodes, ODataNode? parent)
			: base(parent)
		{
			var list = new List<ODataNode>();
			foreach(var node in nodes)
			{
				list.Add(node.WithParentInternal(this));
			}

			Nodes = new ReadOnlyCollection<ODataNode>(list);
		}

		public IReadOnlyCollection<ODataNode> Nodes { get; }

		public OrNode Update(IEnumerable<ODataNode> nodes, ODataNode? parent)
		{
			if (nodes != Nodes || parent != Parent)
			{
				var newNode = new OrNode(nodes, parent);
				return newNode;
			}

			return this;
		}

		public OrNode WithNodes(IEnumerable<ODataNode> nodes)
		{
			return Update(nodes, Parent);
		}

		public OrNode AddNode(ODataNode node)
		{
			var nodes = Nodes.ToList();
			nodes.Add(node);
			return Update(new ReadOnlyCollection<ODataNode>(nodes), Parent);
		}

		public override void Accept(IODataVisitor visitor)
		{
			visitor.VisitOr(this);
		}

		protected internal override ODataNode WithParentInternal(ODataNode? parent)
		{
			return Update(Nodes, parent);
		}
	}
}
