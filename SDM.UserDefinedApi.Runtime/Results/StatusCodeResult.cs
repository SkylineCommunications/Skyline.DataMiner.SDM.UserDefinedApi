namespace Skyline.DataMiner.SDM.UserDefinedApi
{
	using System;

	public class StatusCodeResult : ApiResult
	{
		public StatusCodeResult(int statusCode)
		{
			StatusCode = statusCode;
		}

		public int StatusCode { get; }

		public override void ExecuteResult(ApiContext context)
		{
			if(context is null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			context.Response.ResponseCode = StatusCode;
		}
	}
}
