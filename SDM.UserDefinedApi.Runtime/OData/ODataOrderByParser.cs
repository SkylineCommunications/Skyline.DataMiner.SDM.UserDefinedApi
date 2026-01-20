namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	using System;

	using Skyline.DataMiner.SDM.UserDefinedApi.OData.Exceptions;

	public class ODataOrderByParser
	{
		private readonly ODataTokenizer _tokenizer;

		private ODataToken _token = new ODataToken(ODataTokenType.EndOfToken, String.Empty, 0);

		public ODataOrderByParser(string order)
		{
			_tokenizer = new ODataTokenizer(order);
			Next();
		}

		public OrderNode Parse()
		{
			var node = default(OrderNode);
			if (_token.Type == ODataTokenType.EndOfToken)
			{
				node = new OrderNode(new PropertyNode(String.Empty), ODataOrder.None, null, node);
				return node;
			}

			var order = ParseOrder(null);
			if (_token.Type != ODataTokenType.EndOfToken)
			{
				throw new ODataParseException($"Unexpected token {_token} at the end.");
			}

			return order;
		}

		public OrderNode ParseOrder(OrderNode? parent)
		{
			if (_token.Type != ODataTokenType.Identifier)
			{
				throw new ODataParseException($"Unexpected token {_token} at {_token.Position}");
			}

			var property = new PropertyNode(_token.Text);
			Next();

			ODataOrder order = ODataOrder.Ascending;
			if (_token.Type == ODataTokenType.Ascending)
			{
				order = ODataOrder.Ascending;
				Next();
			}
			else if (_token.Type == ODataTokenType.Descending)
			{
				order = ODataOrder.Descending;
				Next();
			}

			OrderNode? next = null;
			if (_token.Type == ODataTokenType.Comma)
			{
				Next();
				next = ParseOrder(parent);
			}

			return new OrderNode(property, order, next, parent);
		}

		private void Next()
		{
			_token = _tokenizer.NextToken();
		}
	}
}
