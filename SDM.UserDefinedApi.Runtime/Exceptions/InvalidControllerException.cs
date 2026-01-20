namespace Skyline.DataMiner.SDM.UserDefinedApi.Exceptions
{
	using System;
	using System.Runtime.Serialization;

	[Serializable]
	public class InvalidControllerException : Exception
	{
		public InvalidControllerException()
		{
		}

		public InvalidControllerException(string message) : base(message)
		{
		}

		public InvalidControllerException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidControllerException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
