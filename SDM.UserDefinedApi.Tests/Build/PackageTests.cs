namespace Skyline.DataMiner.SDM.UserDefinedApi.Tests.Build
{
	using System.IO;

	using FluentAssertions;

	using Microsoft.Build.Locator;

	using Skyline.DataMiner.SDM.UserDefinedApi;

	[TestClass]
	public class PackageTests
	{
		[TestMethod]
		public void Package_Generate()
		{
			var ticketingDir = @"C:\GIT\Solutions\Generic\Ticketing\SLC-S-Ticketing\SLC-TKT-UDAPI-Ticketing";
			if (!Directory.Exists(ticketingDir))
			{
				Assert.Inconclusive($"Directory '{ticketingDir}' does not exist.");
			}

			var sourceFiles = Directory.GetFiles(@"C:\GIT\Solutions\Generic\Ticketing\SLC-S-Ticketing\SLC-TKT-UDAPI-Ticketing", "*.cs", SearchOption.AllDirectories);
			var references = new[]
			{
				@"C:\Users\ArneMA\.nuget\packages\alphafs.new\2.3.0\lib\net47\AlphaFS.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.files.dataminermessagebroker.api\10.5.3\ref\net462\DataMinerMessageBroker.API.dll",
				@"C:\Users\ArneMA\.nuget\packages\sharpziplib\1.4.2\lib\netstandard2.0\ICSharpCode.SharpZipLib.dll",
				@"C:\Users\ArneMA\.nuget\packages\microsoft.bcl.cryptography\9.0.2\lib\net462\Microsoft.Bcl.Cryptography.dll",
				@"C:\Users\ArneMA\.nuget\packages\newtonsoft.json\13.0.3\lib\net45\Newtonsoft.Json.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.cicd.filesystem\1.1.0\lib\netstandard2.0\Skyline.DataMiner.CICD.FileSystem.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.connectorapi.skylinelockmanager\1.1.2-ama1\lib\net462\Skyline.DataMiner.ConnectorAPI.SkylineLockManager.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.core.dataminersystem.common\1.1.3.3\lib\net462\Skyline.DataMiner.Core.DataMinerSystem.Common.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.core.interappcalls.common\1.0.1.1\lib\net462\Skyline.DataMiner.Core.InterAppCalls.Common.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.sdm.abstractions\0.0.0-b011\lib\net48\Skyline.DataMiner.SDM.Abstractions.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.sdm.ticketing\0.0.6-alpha4\lib\net48\Skyline.DataMiner.SDM.Ticketing.dll",
				//@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.sdm.userdefinedapi\0.0.0-b122\lib\net48\Skyline.DataMiner.SDM.UserDefinedApi.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.sdm.userdefinedapi.runtime\0.0.0-b114\lib\net48\Skyline.DataMiner.SDM.UserDefinedApi.Runtime.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.files.skyline.dataminer.storage.types\10.5.3\ref\net462\Skyline.DataMiner.Storage.Types.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.utils.securecoding\2.1.0\lib\netstandard2.0\Skyline.DataMiner.Utils.SecureCoding.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.files.slanalyticstypes\10.5.3\lib\net462\SLAnalyticsTypes.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.files.slloggerutil\10.5.3\ref\net462\SLLoggerUtil.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.files.slmanagedautomation\10.5.3\lib\net462\SLManagedAutomation.dll",
				@"C:\Users\ArneMA\.nuget\packages\skyline.dataminer.files.slnettypes\10.5.3\ref\net462\SLNetTypes.dll",
				@"C:\Users\ArneMA\.nuget\packages\system.buffers\4.5.1\ref\net45\System.Buffers.dll",
				@"C:\Users\ArneMA\.nuget\packages\system.formats.asn1\9.0.2\lib\net462\System.Formats.Asn1.dll",
				@"C:\Users\ArneMA\.nuget\packages\system.memory\4.5.5\lib\net461\System.Memory.dll",
				@"C:\Users\ArneMA\.nuget\packages\system.numerics.vectors\4.5.0\ref\net46\System.Numerics.Vectors.dll",
				@"C:\Users\ArneMA\.nuget\packages\system.runtime.compilerservices.unsafe\6.0.0\lib\net461\System.Runtime.CompilerServices.Unsafe.dll",
				@"C:\Users\ArneMA\.nuget\packages\system.security.cryptography.pkcs\9.0.2\lib\net462\System.Security.Cryptography.Pkcs.dll",
				@"C:\Users\ArneMA\.nuget\packages\system.threading.tasks.dataflow\7.0.0\lib\net462\System.Threading.Tasks.Dataflow.dll",
				@"C:\Users\ArneMA\.nuget\packages\system.threading.tasks.extensions\4.5.2\lib\netstandard2.0\System.Threading.Tasks.Extensions.dll",
				@"C:\Users\ArneMA\.nuget\packages\system.valuetuple\4.5.0\ref\net47\System.ValueTuple.dll",
				typeof(IEnumerable<>).Assembly.Location,
			};

			var action = () =>
			{
				PackageTask.CreatePackage(
					Path.Combine(ticketingDir, "bin", "UnitTest.Install.dmapp"),
					ticketingDir,
					Path.GetFileNameWithoutExtension(ticketingDir),
					"0.0.0",
					"", // @"C:\GIT\Solutions\Generic\Ticketing\SLC-S-Ticketing\SLC-TKT-UDAPI-Ticketing\bin\Debug\net48\openapi\openapi.yaml",
					sourceFiles,
					references,
					new TestLogCollector());
			};

			action.Should().NotThrow();
		}
	}
}
