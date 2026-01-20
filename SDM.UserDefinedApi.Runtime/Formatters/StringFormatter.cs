namespace Skyline.DataMiner.SDM.UserDefinedApi.Formatters
{
	using System;

	public class StringFormatter : IInputConverter, IOutputConverter
	{
		public string OutputMediaType { get; } = "text/plain";

		public string InputMediaType { get; } = "text/plain";

		public bool CanConvertInput(Type type) => true;

		public bool CanConvertOutput(Type type) => true;

		public object? ConvertInput(string input, Type type)
		{
			return input;
		}

		public string ConvertOutput(object value, Type type)
		{
			return Convert.ToString(value);
		}
	}
}
