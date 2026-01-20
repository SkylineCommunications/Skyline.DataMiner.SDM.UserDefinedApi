namespace Skyline.DataMiner.SDM.UserDefinedApi.Install
{
	using System;
	using System.Text;

	internal class OpenApiInstallBuilder
	{
		private string _scriptName = String.Empty;

		public string ScriptName { get => _scriptName; }

		public OpenApiInstallBuilder WithScriptName(string scriptName)
		{
			_scriptName = scriptName;
			return this;
		}

		public string Build()
		{
			var builder = new StringBuilder("""
namespace UDAPI
{
	using System;
	using System.IO;

	using Skyline.AppInstaller;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.Advanced;

	internal enum FileSyncType
	{
		Changed = 32,
		Removed = 33,
		Added = 34,
	}

	internal class OpenApiInstaller
	{

""");

			builder.AppendFormat("\t\tprivate const string ScriptName = \"{0}\";", _scriptName);
			builder.AppendLine("""
		private const string OpenApiPath = @"C:\Skyline DataMiner\Webpages\Public\SDM\OpenAPI\Scripts";
		private readonly IConnection _connection;
		private readonly AppInstaller _installer;

		public OpenApiInstaller(IConnection connection, AppInstaller installer)
		{
			_connection = connection ?? throw new ArgumentNullException(nameof(connection));
			_installer = installer ?? throw new ArgumentNullException(nameof(installer));
		}

		public void InstallDefaultContent()
		{
			if (String.IsNullOrEmpty(ScriptName))
			{
				// ScriptName is empty when an issue occured so nothing to do.
				return;
			}

			var setupContentPath = _installer.GetSetupContentDirectory();
			var openApiFiles = Directory.GetFiles(setupContentPath, "openapi.*", SearchOption.TopDirectoryOnly);
			if(openApiFiles.Length <= 0)
			{
				// No OpenApi file found in the setup content.
				return;
			}

			Log("Installing the OpenApi specification...");
			if (!Directory.Exists(OpenApiPath))
			{
				Directory.CreateDirectory(OpenApiPath);
			}

			foreach (var file in openApiFiles)
			{
				var destinationPath = Path.Combine(OpenApiPath, $"{ScriptName}{Path.GetExtension(file)}");
				if (!File.Exists(file))
				{
					File.Copy(file, destinationPath);
					SyncFile(_connection, destinationPath, FileSyncType.Added);
				}
				else
				{
					File.Copy(file, destinationPath, true);
					SyncFile(_connection, destinationPath, FileSyncType.Changed);
				}
			}

			Log("Successfully installed the OpenApi specification.");
		}

		private void SyncFile(IConnection connection, string filePath, FileSyncType fileSyncType)
		{
			SetDataMinerInfoMessage message = new SetDataMinerInfoMessage
			{
				What = 41,
				StrInfo1 = filePath,
				IInfo2 = (int)fileSyncType,
			};

			var response = connection.HandleSingleResponseMessage(message);
			if (response == null)
			{
				Log($"Could not sync file, did not receive a response. Path: {filePath}");
			}

			if (response is CreateProtocolFileResponse createProtocolFileResponse && createProtocolFileResponse.ErrorCode != 0)
			{
				Log($"Could not sync file, the returned error code was {createProtocolFileResponse.ErrorCode}. Path: {filePath}");
			}
		}

		private void Log(string message)
		{
			_installer.Log($"[UDAPI.Installer] {message}");
		}
	}
}
""");

			return builder.ToString();
		}
	}
}
