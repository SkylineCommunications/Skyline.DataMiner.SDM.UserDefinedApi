namespace Skyline.DataMiner.SDM.UserDefinedApi.OData
{
	public class ODataToken
	{
		public ODataToken(ODataTokenType type, string txt, int pos)
		{
			Type = type;
			Text = txt;
			Position = pos;
		}

		public ODataTokenType Type { get; }

		public string Text { get; }

		public int Position { get; }

		public override string ToString()
		{
			return $"{Type}('{Text}')";
		}
	}
}
