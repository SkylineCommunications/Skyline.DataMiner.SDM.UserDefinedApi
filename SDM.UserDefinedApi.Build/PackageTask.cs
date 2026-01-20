namespace Skyline.DataMiner.SDM.UserDefinedApi
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp;
	using Microsoft.CodeAnalysis.CSharp.Syntax;

	using Skyline.AppInstaller;
	using Skyline.DataMiner.CICD.FileSystem;
	using Skyline.DataMiner.CICD.Loggers;
	using Skyline.DataMiner.SDM.Parsers.Common.Classes;
	using Skyline.DataMiner.SDM.UserDefinedApi.Install;
	using Skyline.DataMiner.SDM.UserDefinedApi.OpenApi;

	/// <summary>
	/// MSBuild task for generating an OpenAPI specification from C# controller classes.
	/// </summary>
	public class PackageTask : Task
	{
		/// <summary>
		/// Gets or sets the root directory of the project to scan for controllers.
		/// </summary>
		[Required]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
		public string ProjectDirectory { get; set; }

		/// <summary>
		/// Gets or sets the output path (relative to <see cref="ProjectDirectory"/>) where the OpenAPI file will be written.
		/// </summary>
		[Required]
		public string OutputPath { get; set; }

		/// <summary>
		/// Gets or sets the name of the project, used as the OpenAPI document title.
		/// </summary>
		[Required]
		public string ProjectName { get; set; }

		/// <summary>
		/// Gets or sets the version of the project, used as the OpenAPI document version.
		/// </summary>
		[Required]
		public string ProjectVersion { get; set; }

		[Required]
		public ITaskItem[] SourceFiles { get; set; }

		[Required]
		public ITaskItem[] References { get; set; }

		[Required]
		public string OpenApiFile { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

		/// <summary>
		/// Executes the OpenAPI generation task.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the OpenAPI file was generated successfully; otherwise, <c>false</c>.
		/// </returns>
		public override bool Execute()
		{
			try
			{
				Log.LogMessage(MessageImportance.High, "Creating UDAPI Install Package...");

				var outputPath = Path.Combine(ProjectDirectory, OutputPath, $"{ProjectName}.dmapp");
				Log.LogMessage(outputPath);

				CreatePackage(
					outputPath,
					ProjectDirectory,
					ProjectName,
					ProjectVersion,
					OpenApiFile,
					SourceFiles.Select(f => f.ItemSpec),
					References.Select(f => f.ItemSpec),
					new InternalLogCollector(Log));

				Log.LogMessage(MessageImportance.High, $"Created UDAPI Install Package file at {outputPath}");
				return true;
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
				Log.LogMessage(MessageImportance.High, ex.Message);
				Log.LogMessage(MessageImportance.High, ex.StackTrace);
				return false;
			}
		}

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
#pragma warning disable S107 // Methods should not have too many parameters
		internal static void CreatePackage(
			string outputFile,
			string projectDir,
			string projectName,
			string projectVersion,
			string openApiFile,
			IEnumerable<string> sourceFiles,
			IEnumerable<string> references,
			ILogCollector logger)
#pragma warning restore S107 // Methods should not have too many parameters
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
		{
			logger.ReportDebug(outputFile);
			logger.ReportDebug(projectDir);
			logger.ReportDebug(projectName);
			logger.ReportDebug(projectVersion);

			// Make the compilation
			var syntaxTrees = sourceFiles
				.Select(file => CSharpSyntaxTree.ParseText(
					File.ReadAllText(file)))
				.ToList();

			var metadataRefs = references
				.Select(r => MetadataReference.CreateFromFile(r))
				.ToList();

			var compilation = CSharpCompilation.Create(
				"OpenApi",
				syntaxTrees,
				metadataRefs,
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

			// Look for controllers
			var controllers = new List<ControllerUnit>();
			foreach (var tree in syntaxTrees)
			{
				var model = compilation.GetSemanticModel(tree);
				var root = tree.GetRoot();

				// Handle controllers
				var classNodes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
				foreach (var cds in classNodes)
				{
					var attributes = cds.AttributeLists
						.SelectMany(al => al.Attributes)
						.Select(a => a.Name.ToString())
						.ToList();

					if (!attributes.Contains("ApiController") || !attributes.Contains("Route"))
						continue;

					if (cds.BaseList?.Types.Any(t => t.Type.ToString() == "ControllerBase") != true)
						continue;

					var @class = ClassClass.Parse(cds);
					controllers.Add(new ControllerUnit(
						@class,
						model,
						compilation));
				}
			}

			// Generate the OpenApi specification
			var scriptXmls = FileSystem.Instance.Directory.GetFiles(projectDir, $"{projectName}.xml", SearchOption.TopDirectoryOnly);
			if (scriptXmls.Length <= 0)
			{
				logger.ReportError("No Automation Script xml file found that matches the project name.");
				return;
			}

			if (scriptXmls.Length > 1)
			{
				logger.ReportError("Multiple Automation Script xml files found that mathces the project name");
				return;
			}

			var script = Skyline.DataMiner.CICD.Parsers.Automation.Xml.Script.Load(scriptXmls[0]);

			using (var creator = new PackageCreator(
				projectName ?? "User Defined API",
				projectVersion ?? "1.0.0",
				script.Name,
				"10.4.1.0-13639",
				logger))
			{
				var builder = creator.CreateBuilder(controllers);
				if (builder is null)
				{
					logger.ReportError("Failed to create the install package builder (builder was null).");
					return;
				}

				if (File.Exists(openApiFile))
				{
					builder = (AppPackage.AppPackageBuilder)builder.WithSetupFiles(Path.GetDirectoryName(openApiFile));
				}

				var package = builder.Build();
				if (package is null)
				{
					logger.ReportError("Failed to create the install package (package was null).");
					return;
				}

				if (!Directory.Exists(Path.GetDirectoryName(outputFile)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
				}

				package.CreatePackage(outputFile);
			}
		}
	}
}
