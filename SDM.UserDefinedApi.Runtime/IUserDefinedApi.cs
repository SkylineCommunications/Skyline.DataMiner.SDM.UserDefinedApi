namespace Skyline.DataMiner.SDM.UserDefinedApi
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.SDM.UserDefinedApi.Formatters;

	/// <summary>
	/// Represents a user-defined API that can route requests to registered controllers.
	/// </summary>
	public interface IUserDefinedApi
	{
		/// <summary>
		/// Gets the default input converter used by the user-defined API.
		/// </summary>
		/// <value>
		/// The default <see cref="IInputConverter"/> instance used to convert input data.
		/// </value>
		IInputConverter DefaultInputConverter { get; }

		/// <summary>
		/// Gets the default output formatter used by the user-defined API.
		/// </summary>
		/// <value>
		/// The default <see cref="IOutputConverter"/> instance used to format output data.
		/// </value>
		IOutputConverter DefaultOutputConverter { get; }

		/// <summary>
		/// Gets a read-only list of input converters available to the user-defined API.
		/// </summary>
		/// <value>
		/// A read-only list of <see cref="IInputConverter"/> instances.
		/// </value>
		IReadOnlyList<IInputConverter> InputConverters { get; }

		/// <summary>
		/// Gets a read-only list of output converters available to the user-defined API.
		/// </summary>
		/// <value>
		/// A read-only list of <see cref="IOutputConverter"/> instances.
		/// </value>
		IReadOnlyList<IOutputConverter> OutputConverters { get; }

		/// <summary>
		/// Executes the API for the specified trigger input and returns the output.
		/// </summary>
		/// <param name="engine">The DataMiner automation engine instance.</param>
		/// <param name="apiTriggerInput">The API trigger input containing request data.</param>
		/// <returns>
		/// An <see cref="ApiTriggerOutput"/> containing the result of the API execution.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when no matching route handler is found or the API action returns a null result.
		/// </exception>
		ApiTriggerOutput Run(IEngine engine, ApiTriggerInput apiTriggerInput);
	}
}
