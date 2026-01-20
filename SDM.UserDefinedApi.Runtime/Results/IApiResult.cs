namespace Skyline.DataMiner.SDM.UserDefinedApi
{
	public interface IApiResult
	{
		void ExecuteResult(ApiContext context);
	}

	public interface IApiResult<out T> : IApiResult
	{
		T Value { get; }
	}
}
