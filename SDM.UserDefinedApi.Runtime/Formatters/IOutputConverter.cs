namespace Skyline.DataMiner.SDM.UserDefinedApi
{
	using System;

	public interface IOutputConverter
	{
		string OutputMediaType { get; }

		bool CanConvertOutput(Type type);

		string ConvertOutput(object value, Type type);
	}
}
