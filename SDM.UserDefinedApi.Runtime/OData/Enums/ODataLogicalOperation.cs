namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	using Skyline.DataMiner.SDM.UserDefinedApi.OData.Exceptions;

	public enum ODataLogicalOperation
	{
		Equal,
		NotEqual,
		GreaterThan,
		GreaterThanOrEqual,
		LessThan,
		LessThanOrEqual,
		And,
		Or,
		Not,
	}

	internal static class ODataLogicalOperationExtensions
	{
		public static ODataLogicalOperation ToLogicalOperator(this ODataToken token)
		{
			if (!token.Type.IsLogicalToken())
			{
				throw new ODataParseException("Could not parse token to ODataLogationOperation");
			}

			return token.Text switch
			{
				"eq" => ODataLogicalOperation.Equal,
				"ne" => ODataLogicalOperation.NotEqual,
				"gt" => ODataLogicalOperation.GreaterThan,
				"ge" => ODataLogicalOperation.GreaterThanOrEqual,
				"lt" => ODataLogicalOperation.LessThan,
				"le" => ODataLogicalOperation.LessThanOrEqual,
				"not" => ODataLogicalOperation.Not,

				_ => throw new ODataParseException("Could not parse token to ODataLogationOperation"),
			};
		}
	}
}
