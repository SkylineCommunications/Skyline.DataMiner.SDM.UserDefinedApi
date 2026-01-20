namespace Skyline.DataMiner.SDM.UserDefinedApi.Tests.Build
{
	using System;
	using System.IO;

	using Microsoft.Build.Locator;
	using Microsoft.OpenApi;

	using Skyline.DataMiner.SDM.UserDefinedApi;

	[TestClass]
	public class OpenApiTests
	{
		[AssemblyInitialize]
		public static void AssemblyInit(TestContext context)
		{
			if (!MSBuildLocator.IsRegistered)
			{
				MSBuildLocator.RegisterDefaults();
			}
		}

		[TestMethod]
		public void OpenApi_Generate()
		{
			//var ticketingDir = @"C:\GIT\Solutions\Generic\Ticketing\SLC-S-Ticketing\SLC-TKT-UDAPI-Ticketing";
			var ticketingDir = @"C:\GIT\Solutions\Generic\SDM\SLC-S-SDM-Registration\SLC-SDM-UDAPI-Registration";
			if (!Directory.Exists(ticketingDir))
			{
				Assert.Inconclusive($"Directory '{ticketingDir}' does not exist.");
			}

			var sourceFiles = Directory.GetFiles(ticketingDir, "*.cs", SearchOption.AllDirectories);
			var references = new[]
			{
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.files.dataminermessagebroker.api\10.5.11.1\ref\net462\DataMinerMessageBroker.API.dll",
				@"C:\Users\ArneMA\.nuget\packages\microsoft.bcl.asyncinterfaces\10.0.1\lib\net462\Microsoft.Bcl.AsyncInterfaces.dll",
				@"C:\Users\ArneMA\.nuget\packages\microsoft.extensions.dependencyinjection.abstractions\10.0.1\lib\net462\Microsoft.Extensions.DependencyInjection.Abstractions.dll",
				@"C:\Users\ArneMA\.nuget\packages\microsoft.extensions.dependencyinjection\10.0.1\lib\net462\Microsoft.Extensions.DependencyInjection.dll",
				@"C:\Users\ArneMA\.nuget\packages\microsoft.extensions.logging.abstractions\10.0.1\lib\net462\Microsoft.Extensions.Logging.Abstractions.dll",
				@"C:\Users\ArneMA\.nuget\packages\microsoft.extensions.logging\10.0.1\lib\net462\Microsoft.Extensions.Logging.dll",
				@"C:\Users\ArneMA\.nuget\packages\microsoft.extensions.options\10.0.1\lib\net462\Microsoft.Extensions.Options.dll",
				@"C:\Users\ArneMA\.nuget\packages\microsoft.extensions.primitives\10.0.1\lib\net462\Microsoft.Extensions.Primitives.dll",
				@"C:\Users\ArneMA\.nuget\packages\newtonsoft.json\13.0.3\lib\net45\Newtonsoft.Json.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.sdm.abstractions\0.3.0-b002\lib\net48\Skyline.DataMiner.SDM.Abstractions.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.sdm.registration.automation\0.1.0-b006\lib\net48\Skyline.DataMiner.SDM.Registration.Automation.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.sdm.registration.common\0.1.0-b006\lib\net48\Skyline.DataMiner.SDM.Registration.Common.dll",
				////@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.sdm.userdefinedapi\0.3.0-b018\lib\net48\Skyline.DataMiner.SDM.UserDefinedApi.dll",
				////@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.sdm.userdefinedapi.runtime\0.3.0-b018\lib\net48\Skyline.DataMiner.SDM.UserDefinedApi.Runtime.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.files.skyline.dataminer.storage.types\10.5.11.1\ref\net462\Skyline.DataMiner.Storage.Types.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.files.slanalyticstypes\10.5.11.1\lib\net462\SLAnalyticsTypes.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.files.slloggerutil\10.5.11.1\ref\net462\SLLoggerUtil.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.files.slmanagedautomation\10.5.11.1\lib\net462\SLManagedAutomation.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.files.slnettypes\10.5.11.1\ref\net462\SLNetTypes.dll",
				@"C:\Users\ArneMA\.nuget\packages\system.buffers\4.6.1\lib\net462\System.Buffers.dll",
				@"C:\Users\ArneMA\.nuget\packages\system.diagnostics.diagnosticsource\10.0.1\lib\net462\System.Diagnostics.DiagnosticSource.dll",
				@"C:\Users\ArneMA\.nuget\packages\system.memory\4.6.3\lib\net462\System.Memory.dll",
				@"C:\Users\ArneMA\.nuget\packages\system.numerics.vectors\4.6.1\lib\net462\System.Numerics.Vectors.dll",
				@"C:\Users\ArneMA\.nuget\packages\system.runtime.compilerservices.unsafe\6.1.2\lib\net462\System.Runtime.CompilerServices.Unsafe.dll",
				@"C:\Users\ArneMA\.nuget\packages\system.threading.tasks.extensions\4.6.3\lib\net462\System.Threading.Tasks.Extensions.dll",
				typeof(ControllerBase).Assembly.Location,
				typeof(IEnumerable<>).Assembly.Location,
			};

			var doc = OpenApiTask.CreateDocument("TicketType UDAPI Unit Test", "0.0.0-unittest", sourceFiles, references, Console.WriteLine);
			using var sw = new StringWriter();
			var writer = new OpenApiYamlWriter(sw);
			doc.SerializeAsV3(writer);
			var result = sw.ToString();

			Assert.IsNotNull(result);
		}
	}
}
