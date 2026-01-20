namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	public class ODataWalker : ODataVisitor
	{
		public override void VisitAnd(AndNode node)
		{
			DefaultVisit(node);

			foreach (var child in node.Nodes)
			{
				child.Accept(this);
			}
		}

		public override void VisitBinary(BinaryNode node)
		{
			DefaultVisit(node);

			node.Left.Accept(this);
			node.Right.Accept(this);
		}

		public override void VisitFilter(FilterNode node)
		{
			DefaultVisit(node);

			node.Filter?.Accept(this);
		}

		public override void VisitOr(OrNode node)
		{
			DefaultVisit(node);

			foreach (var child in node.Nodes)
			{
				child.Accept(this);
			}
		}

		public override void VisitOrder(OrderNode node)
		{
			DefaultVisit(node);

			node.Next?.Accept(this);
		}

		public override void VisitFunctionCall(FunctionCallNode node)
		{
			DefaultVisit(node);

			foreach(var argument in node.Arguments)
			{
				argument.Accept(this);
			}
		}

		public override void VisitMethodCall(MethodCallNode node)
		{
			DefaultVisit(node);

			node.Target.Accept(this);
			node.Variable.Accept(this);
			node.Body.Accept(this);
		}
	}
}
