namespace Skyline.DataMiner.SDM.UserDefinedApi
{
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.SDM.UserDefinedApi.Formatters;

	public abstract class ControllerBase
	{
		public ApiContext ApiContext { get; internal set; } = new ApiContext();

		public ApiTriggerInput Request => ApiContext.Request;

		public ApiTriggerOutput Response => ApiContext.Response;

		public virtual IInputConverter DefaultInputFormatter { get => ApiContext.DefaultInputConverter; }

		public virtual IOutputConverter DefaultOutputFormatter { get => ApiContext.DefaultOutputConverter; }

		public StatusCodeResult StatusCode(int statusCode)
		{
			return new StatusCodeResult(statusCode);
		}

		public ObjectResult<T> StatusCode<T>(int statusCode, T value)
		{
			return new ObjectResult<T>(statusCode, value)
			{
				Converter = DefaultOutputFormatter,
			};
		}

		public StatusCodeResult Ok()
		{
			return new StatusCodeResult(200);
		}

		public ObjectResult<T> Ok<T>(T value)
		{
			return new ObjectResult<T>(200, value)
			{
				Converter = DefaultOutputFormatter,
			};
		}

		public StatusCodeResult NotFound()
		{
			return new StatusCodeResult(404);
		}

		public ObjectResult<T> NotFound<T>(T value)
		{
			return new ObjectResult<T>(404, value)
			{
				Converter = DefaultOutputFormatter,
			};
		}

		public StatusCodeResult Unauthorized()
		{
			return new StatusCodeResult(401);
		}

		public StatusCodeResult BadRequest()
		{
			return new StatusCodeResult(400);
		}

		public ObjectResult<T> BadRequest<T>(T error)
		{
			return new ObjectResult<T>(400, error)
			{
				Converter = DefaultOutputFormatter,
			};
		}
	}
}
