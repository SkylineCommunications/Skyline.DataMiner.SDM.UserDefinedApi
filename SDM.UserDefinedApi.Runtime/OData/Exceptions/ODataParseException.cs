namespace Skyline.DataMiner.SDM.UserDefinedApi.OData.Exceptions
{
	using System;
	using System.Runtime.Serialization;

	[Serializable]
	public class ODataParseException : Exception
	{
		public ODataParseException()
		{
		}

		public ODataParseException(string message) : base(message)
		{
		}

		public ODataParseException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ODataParseException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
