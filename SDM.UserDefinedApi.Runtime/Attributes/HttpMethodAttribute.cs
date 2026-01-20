namespace Skyline.DataMiner.SDM.UserDefinedApi
{
	using System;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis;

	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public abstract class HttpMethodAttribute : Attribute
	{
		public abstract RequestMethod HttpMethod { get; }
	}
}
