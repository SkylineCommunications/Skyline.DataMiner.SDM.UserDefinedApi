namespace Skyline.DataMiner.SDM.UserDefinedApi
{
	using System;
	using System.Diagnostics;

	using Microsoft.Extensions.Logging;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.SDM.UserDefinedApi.DI;

	internal sealed class EngineLogger<T> : ILogger<T>
	{
		private readonly IEngine _engine;
		private readonly string _categoryName;

		public EngineLogger(IAccessor<IEngine> engineAccessor)
		{
			_engine = engineAccessor.Value;
			_categoryName = typeof(T).FullName ?? "UDAPI.Logger";
		}

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception? exception,
			Func<TState, Exception?, string> formatter)
		{
			if (_engine is null)
			{
				return;
			}

			var message = formatter(state, exception);
			var fullMessage = $"[{logLevel}] {_categoryName}: {message}";

			switch (logLevel)
			{
				case LogLevel.Critical:
				case LogLevel.Error:
					_engine.Log(message, LogType.Error, (int)Skyline.DataMiner.Net.IManager.Helper.LogLevel.NoLogging, GetCallerMethodName());
					break;
				case LogLevel.Warning:
					_engine.Log(message, LogType.Error, (int)Skyline.DataMiner.Net.IManager.Helper.LogLevel.L1, GetCallerMethodName());
					break;
				case LogLevel.Information:
					_engine.Log(message, LogType.Information, (int)Skyline.DataMiner.Net.IManager.Helper.LogLevel.NoLogging, GetCallerMethodName());
					break;
				case LogLevel.Debug:
				case LogLevel.Trace:
					_engine.Log(message, LogType.Debug, (int)Skyline.DataMiner.Net.IManager.Helper.LogLevel.NoLogging, GetCallerMethodName());
					break;
				default:
					_engine.Log(fullMessage);
					break;
			}
		}

		private string GetCallerMethodName()
		{
			var stackTrace = new StackTrace();

			// Skip 2 frames: this method + Log<TState>
			var frame = stackTrace.GetFrame(2);
			return frame?.GetMethod()?.Name ?? "unknown";
		}
	}

	internal sealed class EngineLogger : ILogger
	{
		private readonly IEngine _engine;
		private readonly string _categoryName;

		public EngineLogger(IAccessor<IEngine> engineAccessor)
		{
			_engine = engineAccessor.Value;
			_categoryName = "UDAPI.Logger";
		}

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception? exception,
			Func<TState, Exception?, string> formatter)
		{
			if (_engine is null)
			{
				return;
			}

			var message = formatter(state, exception);
			var fullMessage = $"[{logLevel}] {_categoryName}: {message}";

			switch (logLevel)
			{
				case LogLevel.Critical:
				case LogLevel.Error:
					_engine.Log(message, LogType.Error, (int)Skyline.DataMiner.Net.IManager.Helper.LogLevel.NoLogging, GetCallerMethodName());
					break;
				case LogLevel.Warning:
					_engine.Log(message, LogType.Error, (int)Skyline.DataMiner.Net.IManager.Helper.LogLevel.L1, GetCallerMethodName());
					break;
				case LogLevel.Information:
					_engine.Log(message, LogType.Information, (int)Skyline.DataMiner.Net.IManager.Helper.LogLevel.NoLogging, GetCallerMethodName());
					break;
				case LogLevel.Debug:
				case LogLevel.Trace:
					_engine.Log(message, LogType.Debug, (int)Skyline.DataMiner.Net.IManager.Helper.LogLevel.NoLogging, GetCallerMethodName());
					break;
				default:
					_engine.Log(fullMessage);
					break;
			}
		}

		private string GetCallerMethodName()
		{
			var stackTrace = new StackTrace();

			// Skip 2 frames: this method + Log<TState>
			var frame = stackTrace.GetFrame(2);
			return frame?.GetMethod()?.Name ?? "unknown";
		}

		private sealed class NullScope : IDisposable
		{
			internal static NullScope Instance { get; } = new NullScope();

			public void Dispose()
			{
			}
		}
	}

	internal sealed class NullScope : IDisposable
	{
		internal static NullScope Instance { get; } = new NullScope();

		public void Dispose()
		{
		}
	}
}
