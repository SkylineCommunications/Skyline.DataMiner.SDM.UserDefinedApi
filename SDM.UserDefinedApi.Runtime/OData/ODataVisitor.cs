namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	public class ODataVisitor : IODataVisitor
	{
		public virtual void Visit(ODataNode? node)
		{
			if (node is not null)
			{
				node.Accept(this);
			}
		}

		public virtual void DefaultVisit(ODataNode node)
		{
		}

		public virtual void VisitFilter(FilterNode node)
		{
			DefaultVisit(node);
		}

		public virtual void VisitAnd(AndNode node)
		{
			DefaultVisit(node);
		}

		public virtual void VisitOr(OrNode node)
		{
			DefaultVisit(node);
		}

		public virtual void VisitNot(NotNode node)
		{
			DefaultVisit(node);
		}

		public virtual void VisitBinary(BinaryNode node)
		{
			DefaultVisit(node);
		}

		public virtual void VisitProperty(PropertyNode node)
		{
			DefaultVisit(node);
		}

		public virtual void VisitLiteral(LiteralNode node)
		{
			DefaultVisit(node);
		}

		public virtual void VisitOrder(OrderNode node)
		{
			DefaultVisit(node);
		}

		public virtual void VisitFunctionCall(FunctionCallNode node)
		{
			DefaultVisit(node);
		}

		public virtual void VisitMethodCall(MethodCallNode node)
		{
			DefaultVisit(node);
		}
	}
}
