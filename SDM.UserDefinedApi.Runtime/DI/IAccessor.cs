namespace Skyline.DataMiner.SDM.UserDefinedApi.DI
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;

	public interface IAccessor<T>
	{
		T Value { get; }

		void SetValue(T value);
	}

	public class EngineAccessor : IAccessor<IEngine>
	{
		public IEngine Value { get; private set; }

		public void SetValue(IEngine value)
		{
			if (value is null)
			{
				throw new System.ArgumentNullException(nameof(value));
			}

			Value = value;
		}
	}
}
