namespace Skyline.DataMiner.SDM.UserDefinedApi.OpenApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Microsoft.OpenApi;

	internal class OpenApiGenerator
	{
		internal static OpenApiDocument Create(List<ControllerUnit> controllers, Action<string>? logMethod = null)
		{
			var doc = new OpenApiDocument
			{
				Info = new OpenApiInfo(),
				Paths = new OpenApiPaths(),
				Components = new OpenApiComponents
				{
					Schemas = new Dictionary<string, IOpenApiSchema>(),
					SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
					{
						["BearerAuth"] = new OpenApiSecurityScheme
						{
							Type = SecuritySchemeType.Http,
							Scheme = "bearer",
							In = ParameterLocation.Header,
							Name = "Authorization",
							Description = "The API key you created in DataMiner.",
						},
					},
				},
				Security = new List<OpenApiSecurityRequirement>(),
				Servers = new List<OpenApiServer>
				{
					new OpenApiServer
					{
						Url = "https://{DataMinerSystemName}-{Organization}.on.dataminer.services/api/custom",
						Description = "User Defined API endpoint via cloud connection",
						Variables = new Dictionary<string, OpenApiServerVariable>
						{
							{
								"DataMinerSystemName",
								new OpenApiServerVariable
								{
									Default = String.Empty,
									Description = "The name of the DataMiner System",
								}
							},
							{
								"Organization",
								new OpenApiServerVariable
								{
									Default = String.Empty,
									Description = "The name of the organization",
								}
							},
						},
					},
					new OpenApiServer
					{
						Url = "{Protocol}://{BaseUrl}/api/custom",
						Description = "Local endpoint",
						Variables = new Dictionary<string, OpenApiServerVariable>
						{
							{
								"Protocol",
								new OpenApiServerVariable
								{
									Default = "http",
									Description = "The protocol to use, either http or https",
									Enum = new List<string> { "http", "https" },
								}
							},
							{
								"BaseUrl",
								new OpenApiServerVariable
								{
									Default = "localhost",
									Description = "The base URL of the DataMiner System",
								}
							},
						},
					},
				},
			};

			doc.Security.Add(new OpenApiSecurityRequirement
			{
				[new UserDefinedApiSecurityRequirement("BearerAuth", doc)] = new List<string>(),
			});

			var pathProvider = new PathFactory();
			foreach (var controller in controllers)
			{
				pathProvider.HandleController(doc, controller, logMethod);
			}

			return doc;
		}
	}
}
