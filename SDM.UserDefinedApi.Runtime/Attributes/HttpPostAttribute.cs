namespace Skyline.DataMiner.SDM.UserDefinedApi
{
	using System;

	using Skyline.DataMiner.Net.Apps.UserDefinableApis;

	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public sealed class HttpPostAttribute : HttpMethodAttribute
	{
		public override RequestMethod HttpMethod => RequestMethod.Post;
	}
}
