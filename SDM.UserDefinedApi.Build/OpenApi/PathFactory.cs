namespace Skyline.DataMiner.SDM.UserDefinedApi.OpenApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp.Syntax;
	using Microsoft.OpenApi;

	internal class PathFactory
	{
		private readonly HashSet<ITypeSymbol> _componentTypes;

		internal PathFactory()
		{
			_componentTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
		}

		public void HandleController(OpenApiDocument doc, ControllerUnit unit, Action<string>? logMethod = null)
		{
			if (unit.Class is null || unit.SemanticModel is null)
			{
				return;
			}

			var pathItem = new OpenApiPathItem();
			var basePath = unit.GetRoute();

			var operationProvider = new OperationProvider();
			foreach (var method in unit.Class.Methods
				.Where(m => m.Attributes.Any()))
			{
				// Look for the HTTP Method
				if (!operationProvider.TryGetOperations(unit, method, out var httpMethod, out var operation))
				{
					continue;
				}

				pathItem.AddOperation(httpMethod, operation);

				// Look for components
				// In the return statements
				foreach (var @return in method.OriginalSyntaxNode.DescendantNodes().OfType<ReturnStatementSyntax>())
				{
					// Determine the return type, this could be an array type
					var objectType = TypeHelper.GetReturnType(unit, @return);
					if (objectType is null)
					{
						continue;
					}

					// Check for enumarable and get the item type instead
					objectType = TypeHelper.GetElementType(objectType, unit.Compilation);
					if (objectType is null)
					{
						continue;
					}

					if (!_componentTypes.Add(objectType))
					{
						continue;
					}

					var schema = ComponentFactory.Create(objectType, unit.Compilation);
					if (schema is null)
					{
						continue;
					}

					doc.Components!.Schemas![objectType.Name] = schema;
				}

				// In the parameters
				foreach (var parameter in method.Parameters)
				{
					// For now only support classes in the body
					if (!parameter.Attributes.Any(p => p.Name == "FromBody"))
					{
						continue;
					}

					var typeSymbol = unit.SemanticModel.GetTypeInfo(parameter.OriginalSyntaxNode.Type!).Type;
					var paramType = TypeHelper.GetElementType(typeSymbol!, unit.Compilation);
					if (paramType is null)
					{
						continue;
					}

					if (!_componentTypes.Add(paramType))
					{
						continue;
					}

					var schema = ComponentFactory.Create(paramType, unit.Compilation);
					if (schema is null)
					{
						continue;
					}

					doc.Components!.Schemas![paramType.Name] = schema;
				}
			}

			var path = $"/{basePath.Trim('/')}";

			if (doc.Paths.ContainsKey(path))
			{
				_ = doc.Paths[path].Operations!.Union(pathItem.Operations!);
			}
			else
			{
				doc.Paths.Add(path, pathItem);
			}
		}
	}
}
