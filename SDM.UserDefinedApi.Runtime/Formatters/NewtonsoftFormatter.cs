namespace Skyline.DataMiner.SDM.UserDefinedApi.Formatters
{
	using System;

	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	public class NewtonsoftFormatter : IInputConverter, IOutputConverter
	{
		private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			Converters = { new StringEnumConverter() },
		};

		public string InputMediaType { get; } = "application/json";

		public string OutputMediaType { get; } = "application/json";

		public bool CanConvertInput(Type type) => true;

		public bool CanConvertOutput(Type type) => true;

		public object? ConvertInput(string input, Type type)
		{
			return JsonConvert.DeserializeObject(input, type, Settings);
		}

		public string ConvertOutput(object value, Type type)
		{
			return JsonConvert.SerializeObject(value, Settings);
		}
	}
}
