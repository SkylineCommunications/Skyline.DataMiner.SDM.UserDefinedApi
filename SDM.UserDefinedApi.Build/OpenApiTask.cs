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
	using Microsoft.OpenApi;

	using Skyline.DataMiner.SDM.Parsers.Common.Classes;
	using Skyline.DataMiner.SDM.UserDefinedApi.OpenApi;

	/// <summary>
	/// MSBuild task for generating an OpenAPI specification from C# controller classes.
	/// </summary>
	public class OpenApiTask : Task
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
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

		/// <summary>
		/// Gets or sets the format of the OpenAPI output file. Default is "yaml".<br/>
		/// Options:<br/>
		/// json - Outputs the OpenAPI specification in JSON format.<br/>
		/// yaml - Outputs the OpenAPI specification in YAML format.
		/// </summary>
		public string Format { get; set; } = "yaml";

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
				Log.LogMessage(MessageImportance.High, "Starting OpenAPI generation...");

				var doc = CreateDocument(ProjectName, ProjectVersion, SourceFiles.Select(f => f.ItemSpec), References.Select(f => f.ItemSpec), (message) => Log.LogMessage(MessageImportance.High, message));
				var (fileName, content) = FormatDocument(doc, Format.ToLowerInvariant());

				var outputPath = Path.Combine(ProjectDirectory, OutputPath, fileName);
				if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
				}

				File.WriteAllText(outputPath, content);

				Log.LogMessage(MessageImportance.High, $"OpenAPI file generated at {outputPath}");
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

		internal static OpenApiDocument CreateDocument(
			string projectName,
			string projectVersion,
			IEnumerable<string> sourceFiles,
			IEnumerable<string> references,
			Action<string>? logMethod = null)
		{
			// Make the compilation
			var syntaxTrees = sourceFiles.Select(file => CSharpSyntaxTree.ParseText(
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

			TypeHelper.Load(compilation);

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
			var doc = OpenApiGenerator.Create(controllers, (message) => logMethod?.Invoke(message));
			doc.Info.Title = projectName ?? "User Defined API";
			doc.Info.Version = projectVersion ?? "1.0.0";

			return doc;
		}

		private static (string, string) FormatDocument(OpenApiDocument doc, string format)
		{
			using var sw = new StringWriter();
			if (format.Equals("json", StringComparison.OrdinalIgnoreCase))
			{
				var writer = new OpenApiJsonWriter(sw);
				doc.SerializeAsV3(writer);
				return ("openapi.json", sw.ToString());
			}
			else
			{
				var writer = new OpenApiYamlWriter(sw);
				doc.SerializeAsV3(writer);
				return ("openapi.yaml", sw.ToString());
			}
		}
	}
}
