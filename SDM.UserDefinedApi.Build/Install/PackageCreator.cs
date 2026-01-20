namespace Skyline.DataMiner.SDM.UserDefinedApi.Install
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;

	using Skyline.AppInstaller;
	using Skyline.DataMiner.CICD.Assemblers.Automation;
	using Skyline.DataMiner.CICD.Assemblers.Common;
	using Skyline.DataMiner.CICD.FileSystem;
	using Skyline.DataMiner.CICD.Loggers;
	using Skyline.DataMiner.CICD.Parsers.Automation.Xml;
	using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects;
	using Skyline.DataMiner.SDM.UserDefinedApi.OpenApi;

	internal class PackageCreator : IDisposable
	{
		private readonly string _assemblyPath;
		private readonly string _tempDir;

		private readonly string _projectName;
		private readonly string _version;
		private readonly string _scriptName;
		private readonly string _minimumRequiredDataMinerVersion;
		private readonly ILogCollector _logger;
		private bool disposedValue;

		public PackageCreator(
			string projectName,
			string version,
			string scriptName,
			string minimumRequiredDataMinerVersion,
			ILogCollector logHelper)
		{
			_projectName = projectName ?? throw new ArgumentNullException(nameof(projectName));
			_version = version ?? throw new ArgumentNullException(nameof(version));
			_scriptName = scriptName ?? throw new ArgumentNullException(nameof(scriptName));
			_minimumRequiredDataMinerVersion = minimumRequiredDataMinerVersion ?? throw new ArgumentNullException(nameof(minimumRequiredDataMinerVersion));
			_logger = logHelper;

			_assemblyPath = Path.GetDirectoryName(typeof(PackageCreator).Assembly.Location);
			_tempDir = FileSystem.Instance.Path.Combine(
				FileSystem.Instance.Path.GetTempPath(),
				Guid.NewGuid().ToString());
			FileSystem.Instance.Directory.CreateDirectory(_tempDir);
			Log(_tempDir);
		}

		public AppPackage.AppPackageBuilder? CreateBuilder(List<ControllerUnit> controllers)
		{
			if (!TryCreateInstallScript(controllers, out var script))
			{
				return null;
			}

			var builder = new AppPackage.AppPackageBuilder(
				_projectName,
				_version,
				_minimumRequiredDataMinerVersion,
				script);

			return builder;
		}

		public bool TryCreateInstallScript(List<ControllerUnit> controllers, out AppPackageScript? script)
		{
			script = null;

			try
			{
				var buildResultItems = BuildScript(controllers);
				if (buildResultItems is null)
				{
					return false;
				}

				var filePath = ConvertToInstallScript(buildResultItems);
				var assemblies = new List<string>();

				IEnumerable<string> nugetAssemblies = buildResultItems.Assemblies.Select(reference => reference.AssemblyPath);
				IEnumerable<(string assemblyFilePath, string destinationFolderPath)> extractAssemblies = AppPackageAutomationScriptBuilderHelper.ExtractAssembliesFromScript(buildResultItems.Document, nugetAssemblies);
				assemblies.AddRange(extractAssemblies.Select(reference => reference.assemblyFilePath));
				assemblies.AddRange(buildResultItems.DllAssemblies.Select(reference => reference.AssemblyPath));

				script = new AppPackageScript(filePath, assemblies);

				return true;
			}
			catch (Exception ex)
			{
				Log($"Unexpected exception during package creation for '{_projectName}': {ex}");
				return false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:Elements should be ordered by access", Justification = "Cleaner this way.")]
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					if (!FileSystem.Instance.Directory.Exists(_tempDir))
					{
						return;
					}

					try
					{
						FileSystem.Instance.Directory.Delete(_tempDir, true);
					}
					catch (Exception ex)
					{
						Log($"Could not delete temporary directory '{_tempDir}'. Exception: {ex}");
					}
				}

				disposedValue = true;
			}
		}

		private BuildResultItems? BuildScript(ICollection<ControllerUnit> controllers)
		{
			try
			{
				var projectName = $"{_projectName}.Install";
				var projectDir = FileSystem.Instance.Path.Combine(_tempDir, projectName);
				FileSystem.Instance.Directory.CreateDirectory(projectDir);
				var projectFile = FileSystem.Instance.Path.Combine(projectDir, $"Install.csproj");
				using (var fs = File.OpenWrite(projectFile))
				using (var rs = FileSystem.Instance.File.Open(FileSystem.Instance.Path.Combine(_assemblyPath, "Install", "Content", "Install.csproj"), FileMode.Open))
				{
					rs.CopyTo(fs);
				}

				var scriptXml = FileSystem.Instance.Path.Combine(projectDir, $"Install.xml");
				using (var fs = File.OpenWrite(scriptXml))
				using (var rs = FileSystem.Instance.File.Open(FileSystem.Instance.Path.Combine(_assemblyPath, "Install", "Content", "Install.xml"), FileMode.Open))
				{
					rs.CopyTo(fs);
				}

				using (var fs = File.OpenWrite(FileSystem.Instance.Path.Combine(projectDir, $"Install.cs")))
				using (var rs = FileSystem.Instance.File.Open(FileSystem.Instance.Path.Combine(_assemblyPath, "Install", "Content", "Install.cs"), FileMode.Open))
				{
					rs.CopyTo(fs);
				}

				using (var fs = File.OpenWrite(FileSystem.Instance.Path.Combine(projectDir, $"UserDefinedApiInstaller.cs")))
				using (var sw = new StreamWriter(fs))
				{
					sw.Write(GenerateInstaller(controllers));
				}

				using (var fs = File.OpenWrite(FileSystem.Instance.Path.Combine(projectDir, $"OpenApiInstaller.cs")))
				using (var sw = new StreamWriter(fs))
				{
					sw.Write(GenerateOpenApiInstaller());
				}

				var project = Project.Load(projectFile);
				var script = Script.Load(scriptXml);
				var scriptProjects = new Dictionary<string, Project>
				{
					["Install"] = project,
				};

				var scriptBuilder = new AutomationScriptBuilder(script, scriptProjects, new List<Script>(), _logger, project.ProjectDirectory);
				var buildTask = scriptBuilder.BuildAsync();
				buildTask.Wait();
				return buildTask.Result;
			}
			catch (Exception ex)
			{
				Log($"Unexpected exception during package creation for '{_projectName}': {ex}");
				return null;
			}
		}

		private string ConvertToInstallScript(BuildResultItems buildResultItems)
		{
			// Create temp file, needs to be called Install.xml
			var tempDir = FileSystem.Instance.Path.Combine(
				_tempDir,
				Guid.NewGuid().ToString());
			FileSystem.Instance.Directory.CreateDirectory(tempDir);
			string filePath = FileSystem.Instance.Path.Combine(tempDir, "Install.xml");

			XDocument doc = XDocument.Parse(buildResultItems.Document);
			var ns = doc.Root.GetDefaultNamespace();

			foreach (var item in doc.Descendants(ns + "Param"))
			{
				// Remove the front part of the path as InstallScript won't look in the usual places
				item.Value = FileSystem.Instance.Path.GetFileName(item.Value);
			}

			FileSystem.Instance.File.WriteAllText(filePath, doc.ToString());
			return filePath;
		}

		private string GenerateInstaller(ICollection<ControllerUnit> controllers)
		{
			var builder = new InstallBuilder();
			foreach (var controller in controllers)
			{
				builder.AddEndpoint(controller.Class.Name, controller.GetRoute(), _scriptName);
			}

			return builder.Build();
		}

		private string GenerateOpenApiInstaller()
		{
			return new OpenApiInstallBuilder()
				.WithScriptName(_scriptName)
				.Build();
		}

		private void Log(string message)
		{
			_logger.ReportLog($"[UDAPI.PackageCreator] {message}");
		}
	}
}
