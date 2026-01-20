namespace Skyline.DataMiner.SDM.UserDefinedApi.Install
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	internal class InstallBuilder
	{
		private readonly List<Endpoint> _endpoints = new List<Endpoint>();

		public InstallBuilder AddEndpoint(string controllerName, string route, string scriptName)
		{
			_endpoints.Add(new Endpoint
			{
				Name = controllerName.Substring(0, controllerName.LastIndexOf("Controller")),
				Route = route,
				ScriptName = scriptName,
			});

			return this;
		}

		public string Build()
		{
			var builder = new StringBuilder("""
namespace UDAPI
{
	using System;
	using System.Linq;

	using Skyline.AppInstaller;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.AppPackages;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	internal class UserDefinedApiInstaller
	{
		private	readonly IConnection _connection;
		private readonly AppInstallContext _context;
		private readonly UserDefinableApiHelper _helper;
		private readonly Action<string> _logger;

		public UserDefinedApiInstaller(IConnection connection, AppInstallContext context)
		{
			_connection = connection ?? throw new ArgumentNullException(nameof(connection));
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_helper = new UserDefinableApiHelper(_connection.HandleMessages);
			_logger = new AppInstaller(_connection, _context).Log;
		}

		internal void InstallDefaultContent()
		{
			Log("Installing UDAPI endpoints...");

""");

			foreach (var endpoint in _endpoints.Select(x => x.Name))
			{
				builder.AppendLine($"\t\t\tInstall{endpoint}Endpoint();");
			}

			builder.Append("""
			Log("UDAPI endpoints installed.");
		}


""");

			foreach (var endpoint in _endpoints)
			{
				builder.Append($$"""
		private void Install{{endpoint.Name}}Endpoint()
		{
			Log("Installing {{endpoint.Name}} UDAPI endpoint...");
			var definition = new ApiDefinition
			{
				Name = "{{endpoint.Name}} UDAPI",
				ActionType = ActionType.AutomationScript,
				Route = "{{endpoint.Route.Trim('/')}}",
				ActionMeta = new AutomationScriptActionMeta
				{
					InputType = InputType.RawBody,
					ScriptName = "{{endpoint.ScriptName}}",
				},
			};

			Import(definition);
			Log("{{endpoint.Name}} UDAPI endpoint installed.");
		}


""");
			}

			builder.Append("""
		private void Import(ApiDefinition definition)
		{
			// Remove other api definition that is using the same route
			var routeUsed = _helper.ApiDefinitions.Read(ApiDefinitionExposers.Route.Equal(definition.Route)).FirstOrDefault();
			if(routeUsed != null && routeUsed.ID.Id != definition.ID.Id)
			{
				_helper.ApiDefinitions.Delete(routeUsed);
			}
				
			// Import or update the api definition
			var existing = _helper.ApiDefinitions.Read(ApiDefinitionExposers.Id.Equal(definition.ID)).FirstOrDefault();
			if(existing is null)
			{
				_helper.ApiDefinitions.Create(definition);
			}
			else
			{
				definition.SecuritySettings = existing.SecuritySettings;
				_helper.ApiDefinitions.Update(definition);
			}
		}
				
		private void Log(string message)
		{
			_logger?.Invoke($"[UDAPI.Installer] {message}");
		}
	}
}
""");

			return builder.ToString();
		}

		private sealed class Endpoint
		{
			public string Name { get; set; } = String.Empty;

			public string Route { get; set; } = String.Empty;

			public string ScriptName { get; set; } = String.Empty;
		}
	}
}
