namespace Skyline.DataMiner.SDM.UserDefinedApi
{
	using System;

	[AttributeUsage(AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
	public sealed class FromBodyAttribute : Attribute
	{
	}
}
