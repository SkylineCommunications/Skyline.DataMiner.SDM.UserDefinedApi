namespace Skyline.DataMiner.SDM.UserDefinedApi
{
	public abstract class ApiResult : IApiResult
	{
		public virtual void ExecuteResult(ApiContext context)
		{
		}
	}
}
