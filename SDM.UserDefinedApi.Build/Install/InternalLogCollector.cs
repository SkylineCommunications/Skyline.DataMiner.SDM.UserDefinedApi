namespace Skyline.DataMiner.SDM.UserDefinedApi.Install
{
	using System;

	using Microsoft.Build.Utilities;

	using Skyline.DataMiner.CICD.Loggers;

	internal class InternalLogCollector : ILogCollector
	{
		private readonly TaskLoggingHelper _helper;

		public InternalLogCollector(TaskLoggingHelper helper)
		{
			_helper = helper ?? throw new ArgumentNullException(nameof(helper));
		}

		public void ReportDebug(string debug)
		{
			_helper.LogMessage($"[DEBUG] {debug}");
		}

		public void ReportError(string error)
		{
			_helper.LogError($"[ERROR] {error}");
		}

		public void ReportLog(string message)
		{
			_helper.LogMessage($"[LOG] {message}");
		}

		public void ReportStatus(string status)
		{
			_helper.LogMessage($"[STATUS] {status}");
		}

		public void ReportWarning(string warning)
		{
			_helper.LogWarning($"[WARNING] {warning}");
		}
	}
}
