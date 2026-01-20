namespace Skyline.DataMiner.SDM.UserDefinedApi.Tests.Build
{
	using System;

	using Skyline.DataMiner.CICD.Loggers;

	internal class TestLogCollector : ILogCollector
	{
		public void ReportDebug(string debug) => Console.WriteLine($"[DEBUG] {debug}");

		public void ReportError(string error) => Console.WriteLine($"[ERROR] {error}");

		public void ReportLog(string message) => Console.WriteLine($"[INFO] {message}");

		public void ReportStatus(string status) => Console.WriteLine($"[STATUS] {status}");

		public void ReportWarning(string warning) => Console.WriteLine($"[WARNING] {warning}");
	}
}
