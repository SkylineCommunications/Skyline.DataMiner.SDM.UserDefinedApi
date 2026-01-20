namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	public enum ODataTokenType
	{
		// General
		EndOfToken = 0,
		SystemQuery = 9,
		Identifier = 10,
		Method = 11,
		LamdaParameter = 12,
		Null = 15,
		Number = 16,
		String = 17,

		// Logicals
		Eq = 100,
		Ne,
		Gt,
		Ge,
		Lt,
		Le,
		And,
		Or,
		Not,

		// Arithmetic
		Add = 200,
		Sub,
		Mul,
		Div,
		Mod,

		// Grouping
		LeftParenthesis = 300,	// '('
		RightParenthesis,       // ')'
		LeftBrace,              // '{'
		RightBrace,             // '}'
		Comma,                  // ','
		Colon,                  // ':'
		Slash,                  // '/'

		// Strings
		SubStringOf = 400,
		EndsWith,
		StartsWith,
		Length,
		IndexOf,
		Replace,
		SubString,
		ToLower,
		ToUpper,
		Trim,
		Concat,

		// Date
		Day = 500,
		Hour,
		Minute,
		Month,
		Second,
		Year,
		Round,
		Floor,
		Ceiling,

		// Type
		IsOf = 600,

		// Order
		Ascending = 700,
		Descending,

		// Collections
		Any = 800,
		All,
	}

	public static class ODataTokenTypeExtensions
	{
		public static bool IsLogicalToken(this ODataToken token) => token.Type.IsLogicalToken();

		public static bool IsLogicalToken(this ODataTokenType type)
		{
			var index = (int)type;
			if (index >= 100 && index <= 199)
			{
				return true;
			}

			return false;
		}

		public static bool IsLiteral(this ODataToken token) => IsLiteral(token.Type);

		public static bool IsLiteral(this ODataTokenType type)
		{
			var index = (int)type;
			if (index > 10 && index <= 99)
			{
				return true;
			}

			return false;
		}
	}
}
