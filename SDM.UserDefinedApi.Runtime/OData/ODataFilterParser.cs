namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;

	using Skyline.DataMiner.SDM.UserDefinedApi.OData.Exceptions;

	public class ODataFilterParser
	{
		private readonly ODataTokenizer _tokenizer;

		private ODataToken _token;

		public ODataFilterParser(string filter)
		{
			_tokenizer = new ODataTokenizer(filter);
			Next();
		}

		public FilterNode Parse()
		{
			var node = new FilterNode();
			if (_token.Type == ODataTokenType.EndOfToken)
			{
				return node; // In case of empty string
			}

			var filter = ParseOr(node);
			if (_token.Type != ODataTokenType.EndOfToken)
			{
				throw new ODataParseException($"Unexpected token {_token} at the end.");
			}

			return new FilterNode(filter);
		}

		private ODataNode ParseOr(ODataNode? parent)
		{
			var node = new OrNode(new List<ODataNode>(), parent);
			var left = ParseAnd(node);
			node = node.AddNode(left);
			while (_token.Type == ODataTokenType.Or)
			{
				Next();
				var right = ParseAnd(node);
				node = node.AddNode(right);
			}

			if (node.Nodes.Count > 1)
			{
				return node;
			}
			else
			{
				return left!.WithParentInternal(parent);
			}
		}

		private ODataNode ParseAnd(ODataNode? parent)
		{
			var node = new AndNode(new List<ODataNode>(), parent);
			var left = ParseNot(node);
			node = node.AddNode(left);
			while (_token.Type == ODataTokenType.And)
			{
				Next();
				var right = ParseNot(node);
				node = node.AddNode(right);
			}

			if (node.Nodes.Count > 1)
			{
				return node;
			}
			else
			{
				return left!.WithParentInternal(parent);
			}
		}

		private ODataNode ParseNot(ODataNode? parent)
		{
			var node = new NotNode(new PropertyNode(String.Empty));
			if (_token.Type != ODataTokenType.Not)
			{
				return ParseFunctionCall(parent);
			}

			Next();
			var inner = ParseNot(node);
			return new NotNode(inner, parent);
		}

		private ODataNode ParseFunctionCall(ODataNode? parent)
		{
			if (_token.Type != ODataTokenType.Method)
			{
				return ParseComparison(parent);
			}

			var methodName = _token.Text;
			Next(); // Consume method name
			if (_token.Type != ODataTokenType.LeftParenthesis)
			{
				throw new ODataParseException($"Unexpected token {_token} at {_token.Position}. Missing opening brace '('");
			}

			Next(); // Consume '('

			var arguments = new List<ODataNode>();
			while (_token.Type != ODataTokenType.RightParenthesis)
			{
				if (_token.Type == ODataTokenType.EndOfToken)
				{
					throw new ODataParseException($"Unexpected token {_token} at {_token.Position}. Missing closing brace ')'");
				}

				arguments.Add(ParseComparison(null));
				if (_token.Type == ODataTokenType.Comma)
				{
					Next();
				}
			}

			Next(); // Consume the ')'

			return new FunctionCallNode(methodName, arguments, parent);
		}

		private MethodCallNode ParseMethodCall(PropertyNode target, ODataNode? parent)
		{
			var methodName = _token.Text;
			Next(); // Consume the method name

			if(_token.Type != ODataTokenType.LeftParenthesis)
			{
				throw new ODataParseException($"Unexpected token {_token} at {_token.Position}. Method call should start with an open parenthesis '('.");
			}

			Next(); // Consume '('
			if(_token.Type != ODataTokenType.LamdaParameter)
			{
				throw new ODataParseException($"Unexpected token {_token} at {_token.Position}. Method call should start with a variable name.");
			}

			var variable = new PropertyNode(_token.Text, null);
			Next(); // Consume the variable name

			if(_token.Type != ODataTokenType.Colon)
			{
				throw new ODataParseException($"Unexpected token {_token} at {_token.Position}. After a variable declaration there should be a colon ':'.");
			}

			Next(); // Consume ':'
			var body = ParseOr(null);

			if(_token.Type != ODataTokenType.RightParenthesis)
			{
				throw new ODataParseException($"Unexpected token {_token} at {_token.Position}. Missing closing brace ')'");
			}

			Next(); // Consume ')'
			return new MethodCallNode(methodName, target, variable, body, parent);
		}

		private ODataNode ParseComparison(ODataNode? parent)
		{
			var node = new BinaryNode(new PropertyNode(String.Empty), ODataLogicalOperation.Equal, new PropertyNode(String.Empty));

			if (_token.Type == ODataTokenType.LeftParenthesis)
			{
				var open = _token.Type;
				Next();
				var inner = ParseOr(node);
				if (open == ODataTokenType.LeftParenthesis && _token.Type != ODataTokenType.RightParenthesis)
				{
					throw new ODataParseException("Missing closing brace ')'");
				}

				Next();
				return inner;
			}

			if (_token.Type.IsLiteral())
			{
				var left = ParseLiteral(parent);
				Next();
				if (_token.Type.IsLogicalToken())
				{
					var op = _token.ToLogicalOperator();
					Next();
					var right = new PropertyNode(_token.Text, node);
					Next();
					node = new BinaryNode(left, op, right, parent);
					return node;
				}

				return left;
			}

			if (_token.Type == ODataTokenType.Identifier)
			{
				var left = new PropertyNode(_token.Text, parent);
				Next(); // Consumme Identifier
				while (_token.Type == ODataTokenType.Slash)
				{
					Next(); // Consume '/'
					if (_token.Type == ODataTokenType.Identifier)
					{
						left = left.WithPath(String.Join("/", left.Path, _token.Text));
						Next(); // Consume Identifier
						continue;
					}

					if(_token.Type == ODataTokenType.Method)
					{
						return ParseMethodCall(left, node);
					}
				}

				if (_token.Type.IsLogicalToken())
				{
					var op = _token.ToLogicalOperator();
					Next();
					var right = ParseLiteral(node);
					Next();
					node = new BinaryNode(left, op, right, parent);
					return node;
				}

				return left;
			}

			throw new NotSupportedException();
		}

		private LiteralNode ParseLiteral(ODataNode? parent)
		{
			object value = null;
			if (_token.Type == ODataTokenType.Number)
			{
				string txt = _token.Text.Replace(",", ".");
				var type = ODataPrimitiveType.Null;
				if (txt.Contains("."))
				{
					if (double.TryParse(txt, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
					{
						value = d;
						type = ODataPrimitiveType.Double;
					}
				}
				else
				{
					if (long.TryParse(txt, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l))
					{
						value = l;
						type = ODataPrimitiveType.Int64;
					}
				}

				// fallback
				if (double.TryParse(txt, NumberStyles.Float, CultureInfo.InvariantCulture, out double dd))
				{
					value = dd;
					type = ODataPrimitiveType.Double;
				}

				if (value is null)
				{
					throw new ODataParseException($"Could not parse number token '{_token}'");
				}

				return new LiteralNode(type, value, parent);
			}

			if (_token.Type == ODataTokenType.String)
			{
				var s = _token.Text;
				return new LiteralNode(ODataPrimitiveType.String, s, parent);
			}

			if (_token.Type == ODataTokenType.Null)
			{
				return new LiteralNode(ODataPrimitiveType.Null, null, parent);
			}

			throw new ODataParseException($"Expected literal but got {_token}");
		}

		private void Next()
		{
			_token = _tokenizer.NextToken();
		}
	}
}
