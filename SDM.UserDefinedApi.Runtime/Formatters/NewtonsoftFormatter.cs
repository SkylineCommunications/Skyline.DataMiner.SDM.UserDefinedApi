namespace Skyline.DataMiner.SDM.UserDefinedApi.Formatters
{
	using System;

	using Newtonsoft.Json;

	public class NewtonsoftFormatter : IInputConverter, IOutputConverter
	{
		public string InputMediaType { get; } = "application/json";

		public string OutputMediaType { get; } = "application/json";

		public bool CanConvertInput(Type type) => true;

		public bool CanConvertOutput(Type type) => true;

		public object? ConvertInput(string input, Type type)
		{
			return JsonConvert.DeserializeObject(input, type);
		}

		public string ConvertOutput(object value, Type type)
		{
			return JsonConvert.SerializeObject(value);
		}
	}
}
