namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	public interface IODataVisitor
	{
		void VisitLiteral(LiteralNode node);
		void VisitProperty(PropertyNode node);
		void VisitFunctionCall(FunctionCallNode node);
		void VisitMethodCall(MethodCallNode node);
		void VisitBinary(BinaryNode node);
		void VisitFilter(FilterNode node);
		void VisitAnd(AndNode node);
		void VisitOr(OrNode node);
		void VisitNot(NotNode node);
		void VisitOrder(OrderNode node);
	}
}
