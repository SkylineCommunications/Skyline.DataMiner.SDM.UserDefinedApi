namespace Skyline.DataMiner.SDM.UserDefinedApi
{
	using System;

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class ApiControllerAttribute : Attribute
	{
	}
}
