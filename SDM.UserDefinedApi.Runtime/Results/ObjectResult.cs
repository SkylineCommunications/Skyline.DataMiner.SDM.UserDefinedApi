namespace Skyline.DataMiner.SDM.UserDefinedApi
{
	using System;
	using System.Linq;

	////public class ObjectResult : StatusCodeResult, IApiResult<object>
	////{
	////	public ObjectResult(int statusCode, object value) : base(statusCode)
	////	{
	////		Value = value;
	////	}

	////	public object Value { get; set; }

	////	public IOutputConverter? Converter { get; set; }

	////	public override void ExecuteResult(ApiContext context)
	////	{
	////		base.ExecuteResult(context);
	////		var converter = Converter;
	////		if (converter is null)
	////		{
	////			converter = context.OutputConverters.Reverse().FirstOrDefault(c => c.CanConvertOutput(Value.GetType()));
	////		}

	////		if(converter is null)
	////		{
	////			throw new NotSupportedException($"Could not convert result of type '{Value.GetType().FullName}', because no valid converter was found.");
	////		}

	////		context.Response.ResponseBody = converter.ConvertOutput(Value, typeof(object));
	////	}
	////}

	public class ObjectResult<T> : StatusCodeResult, IApiResult<T>
	{
		public ObjectResult(int statusCode, T value) : base(statusCode)
		{
			Value = value;
		}

		public T Value { get; set; }

		public IOutputConverter? Converter { get; set; }

		public override void ExecuteResult(ApiContext context)
		{
			base.ExecuteResult(context);
			var converter = Converter;
			if (converter is null)
			{
				converter = context.OutputConverters.Reverse().FirstOrDefault(c => c.CanConvertOutput(typeof(T)));
			}

			if (converter is null)
			{
				throw new NotSupportedException($"Could not convert result of type '{typeof(T).FullName}', because no valid converter was found.");
			}

			context.Response.ResponseBody = converter.ConvertOutput(typeof(T), typeof(object));
		}
	}
}
