namespace Skyline.DataMiner.SDM.UserDefinedApi
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.SDM.UserDefinedApi.Formatters;

	public class ApiContext
	{
		public ApiTriggerInput Request { get; internal set; } = new ApiTriggerInput();

		public ApiTriggerOutput Response { get; internal set; } = new ApiTriggerOutput();

		public IInputConverter DefaultInputConverter { get; internal set; } = new NewtonsoftFormatter();

		public IOutputConverter DefaultOutputConverter { get; internal set; } = new NewtonsoftFormatter();

		public IReadOnlyCollection<IInputConverter> InputConverters { get; internal set; } = new ReadOnlyCollection<IInputConverter>(new List<IInputConverter>());

		public IReadOnlyCollection<IOutputConverter> OutputConverters { get; internal set; } = new ReadOnlyCollection<IOutputConverter>(new List<IOutputConverter>());
	}
}
