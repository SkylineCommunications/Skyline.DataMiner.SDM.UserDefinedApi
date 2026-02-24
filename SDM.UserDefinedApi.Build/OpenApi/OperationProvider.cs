namespace Skyline.DataMiner.SDM.UserDefinedApi.OpenApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;

	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp.Syntax;
	using Microsoft.OpenApi;

	using Skyline.DataMiner.SDM.Parsers.Common.Classes;

	internal class OperationProvider
	{
		public bool TryGetOperations(ControllerUnit unit, MethodClass endpoint, out HttpMethod method, out OpenApiOperation operation)
		{
			method = HttpMethod.Get;
			operation = new OpenApiOperation
			{
				Tags = new HashSet<OpenApiTagReference>(),
			};

			if (endpoint is null)
			{
				return false;
			}

			if (!TryGetHttpMethod(endpoint, out method))
			{
				return false;
			}

			operation.Tags.Add(new OpenApiTagReference(unit.Class.Name.Substring(0, unit.Class.Name.IndexOf("Controller"))));
			operation.Responses = GetResponses(endpoint, unit);
			operation.Parameters = GetParameters(endpoint, unit);
			operation.RequestBody = GetRequestBody(endpoint, unit);

			if (!String.IsNullOrEmpty(endpoint.Documentation?.Summary))
			{
				operation.Summary = endpoint.Documentation!.Summary;
			}

			if (!String.IsNullOrEmpty(endpoint.Documentation?.Example))
			{
				operation.Description = endpoint.Documentation!.Example;
			}

			return true;
		}

		private bool TryGetHttpMethod(MethodClass endpoint, out HttpMethod method)
		{
			method = HttpMethod.Get;
			foreach (var attribute in endpoint.Attributes)
			{
				switch (attribute.Name)
				{
					case "HttpGet":
						method = HttpMethod.Get;
						return true;

					case "HttpPost":
						method = HttpMethod.Post;
						return true;

					case "HttpPut":
						method = HttpMethod.Put;
						return true;

					case "HttpDelete":
						method = HttpMethod.Delete;
						return true;

					case "HttpPatch":
						method = new HttpMethod("Patch");
						return true;

					case "HttpHead":
						method = HttpMethod.Head;
						return true;

					case "HttpOptions":
						method = HttpMethod.Options;
						return true;

					default:
						return false;
				}
			}

			return false;
		}

		private OpenApiResponses GetResponses(MethodClass endpoint, ControllerUnit unit)
		{
			var responses = new OpenApiResponses();

			foreach (var @return in endpoint.OriginalSyntaxNode.DescendantNodes().OfType<ReturnStatementSyntax>())
			{
				int statusCode = -1;
				var response = new OpenApiResponse();

				// Handle the standard returns with the build in helper methods
				// e.g. return Ok(object), return BadRequest(object), etc.
				if (@return.Expression is InvocationExpressionSyntax invocation)
				{
					statusCode = invocation.GetStatusCode(unit);
				}

				// Handle direct returns
				else if (@return.Expression is ObjectCreationExpressionSyntax objectCreation)
				{
					var typeInfo = unit.SemanticModel.GetTypeInfo(objectCreation).Type;
					if (typeInfo is INamedTypeSymbol namedType &&
						namedType.Name == "ObjectResult" &&
						objectCreation.ArgumentList is not null)
					{
						var valueArg = objectCreation.ArgumentList.Arguments[1].Expression;
					}
				}

				if (statusCode < 0)
				{
					continue;
				}

				var objectType = TypeHelper.GetReturnType(unit, @return);
				if (objectType is null)
				{
					continue;
				}

				response.Description = String.Empty;
				response.Content = new Dictionary<string, IOpenApiMediaType>
				{
					["application/json"] = new OpenApiMediaType
					{
						Schema = objectType.ToOpenApiSchema(null, unit),
					},
				};

				responses[statusCode.ToString()] = response;
			}

			return responses;
		}

		private IList<IOpenApiParameter>? GetParameters(MethodClass endpoint, ControllerUnit unit)
		{
			var parameters = new List<IOpenApiParameter>();
			foreach (var parameter in endpoint.Parameters)
			{
				var openApiParameter = new OpenApiParameter();
				if (parameter is null)
				{
					continue;
				}

				openApiParameter.Name = parameter.Name;
				openApiParameter.In = (parameter.Attributes.FirstOrDefault(a => a.Name.StartsWith("From"))?.Name ?? "N/A") switch
				{
					"FromQuery" => ParameterLocation.Query,
					"FromHeader" => ParameterLocation.Header,
					"FromRoute" => ParameterLocation.Path,
					_ => null,
				};

				if (openApiParameter.In is null)
				{
					continue;
				}

				openApiParameter.Required = parameter.DefaultValue is null;
				openApiParameter.Schema = parameter.ToOpenApiSchema(unit);

				if (endpoint.Documentation?.Parameters?.TryGetValue(parameter.Name, out var description) ?? false)
				{
					openApiParameter.Description = description;
				}

				parameters.Add(openApiParameter);
			}

			if (parameters.Count == 0)
			{
				return null;
			}

			return parameters;
		}

		private IOpenApiRequestBody? GetRequestBody(MethodClass endpoint, ControllerUnit unit)
		{
			var bodyParam = endpoint.Parameters.SingleOrDefault(p => p.Attributes.Any(a => a.Name == "FromBody"));
			if (bodyParam is null)
			{
				return null;
			}

			var requestBody = new OpenApiRequestBody
			{
				Content = new Dictionary<string, IOpenApiMediaType>
				{
					["application/json"] = new OpenApiMediaType
					{
						Schema = bodyParam.ToOpenApiSchema(unit),
					},
				},
				Required = bodyParam.DefaultValue is null,
			};
			return requestBody;
		}
	}
}
