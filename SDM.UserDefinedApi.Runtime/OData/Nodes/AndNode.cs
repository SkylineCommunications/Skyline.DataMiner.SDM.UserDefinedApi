namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;

	public class AndNode : ODataNode<AndNode>
	{
		internal AndNode(IEnumerable<ODataNode> nodes, ODataNode? parent)
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

		public AndNode Update(IEnumerable<ODataNode> nodes, ODataNode? parent)
		{
			if (nodes != Nodes || parent != Parent)
			{
				var newNode = new AndNode(nodes, parent);
				return newNode;
			}

			return this;
		}

		public AndNode WithNodes(IEnumerable<ODataNode> nodes)
		{
			return Update(nodes, Parent);
		}

		public AndNode AddNode(ODataNode node)
		{
			var nodes = Nodes.ToList();
			nodes.Add(node);
			return Update(new ReadOnlyCollection<ODataNode>(nodes), Parent);
		}

		public override void Accept(IODataVisitor visitor)
		{
			visitor.VisitAnd(this);
		}

		protected internal override ODataNode WithParentInternal(ODataNode? parent)
		{
			return Update(Nodes, parent);
		}
	}
}
