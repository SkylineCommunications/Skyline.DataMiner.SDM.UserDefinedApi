namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	using System;
	using System.Text;

	using Skyline.DataMiner.SDM.UserDefinedApi.OData.Exceptions;

	public class ODataTokenizer
	{
		private const char END = '\0';
		private readonly string _filter;
		private int i = 0;

		public ODataTokenizer(string filter)
		{
			_filter = filter;
		}

		public ODataToken NextToken()
		{
			SkipWhitespace();
			var start = i;
			var c = Peek();

			if (c == END)
			{
				return new ODataToken(ODataTokenType.EndOfToken, String.Empty, i);
			}

			// Handle Identifiers and other
			if (char.IsLetter(c) || c == '_')
			{
				var sb = new StringBuilder();
				while (char.IsLetterOrDigit(Peek()) || Peek() == '_')
				{
					sb.Append(NextChar());
				}

				// Method call
				if (Peek() == '(')
				{
					return new ODataToken(ODataTokenType.Method, sb.ToString(), start);
				}

				// Lambda param
				if (Peek() == ':')
				{
					return new ODataToken(ODataTokenType.LamdaParameter, sb.ToString(), start);
				}

				var txt = sb.ToString().ToLowerInvariant();
				return txt switch
				{
					"eq" => new ODataToken(ODataTokenType.Eq, txt, start),
					"ne" => new ODataToken(ODataTokenType.Ne, txt, start),
					"gt" => new ODataToken(ODataTokenType.Gt, txt, start),
					"ge" => new ODataToken(ODataTokenType.Ge, txt, start),
					"lt" => new ODataToken(ODataTokenType.Lt, txt, start),
					"le" => new ODataToken(ODataTokenType.Le, txt, start),
					"and" => new ODataToken(ODataTokenType.And, txt, start),
					"or" => new ODataToken(ODataTokenType.Or, txt, start),
					"not" => new ODataToken(ODataTokenType.Not, txt, start),
					"asc" => new ODataToken(ODataTokenType.Ascending, txt, start),
					"desc" => new ODataToken(ODataTokenType.Descending, txt, start),
					"null" => new ODataToken(ODataTokenType.Null, txt, start),
					_ => new ODataToken(ODataTokenType.Identifier, sb.ToString(), start),
				};
			}

			// Handle number literals
			if (char.IsDigit(c))
			{
				var sb = new StringBuilder();
				while (char.IsDigit(Peek()))
				{
					sb.Append(NextChar());
				}

				// In case of double, float or decimal
				if (Peek() == '.' || Peek() == ',')
				{
					sb.Append(NextChar());
					while (char.IsDigit(Peek()))
					{
						sb.Append(NextChar());
					}
				}

				return new ODataToken(ODataTokenType.Number, sb.ToString(), start);
			}

			// Handle string literals
			if (c == '\'')
			{
				NextChar(); // Consume opening quote
				var sb = new StringBuilder();
				while (true)
				{
					var p = Peek();
					if (p == END)
					{
						throw new ODataParseException($"Unterminated string literal at {start}");
					}

					if (p == '\\')
					{
						NextChar(); // Consume '\'
						char esc = NextChar();
						sb.Append(esc switch
						{
							'n' => '\n',
							'r' => '\r',
							't' => '\t',
							'\\' => '\\',
							'\'' => '\'',
							_ => esc,
						});

						continue;
					}

					if (p == '\'')
					{
						NextChar(); // Consume closing quote
						break;
					}

					sb.Append(NextChar());
				}

				return new ODataToken(ODataTokenType.String, sb.ToString(), start);
			}

			return NextChar() switch
			{
				'(' => new ODataToken(ODataTokenType.LeftParenthesis, "(", start),
				')' => new ODataToken(ODataTokenType.RightParenthesis, ")", start),
				'{' => new ODataToken(ODataTokenType.LeftBrace, "{", start),
				'}' => new ODataToken(ODataTokenType.RightBrace, "}", start),
				',' => new ODataToken(ODataTokenType.Comma, ",", start),
				':' => new ODataToken(ODataTokenType.Colon, ":", start),
				'/' => new ODataToken(ODataTokenType.Slash, "/", start),
				_ => throw new ODataParseException($"Unexpected character '{c} at {start}'")
			};
		}

		private char Peek()
		{
			if (i < _filter.Length)
			{
				return _filter[i];
			}

			return END;
		}

		private char PeekNext()
		{
			return i + 1 < _filter.Length ? _filter[i + 1] : END;
		}

		private char NextChar()
		{
			if (i < _filter.Length)
			{
				return _filter[i++];
			}

			return END;
		}

		private void SkipWhitespace()
		{
			while (char.IsWhiteSpace(Peek()))
			{
				i++;
			}
		}
	}
}
