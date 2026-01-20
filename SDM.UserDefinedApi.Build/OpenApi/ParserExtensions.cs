namespace Skyline.DataMiner.SDM.UserDefinedApi.OpenApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.Json.Nodes;

	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp.Syntax;
	using Microsoft.OpenApi;

	using Skyline.DataMiner.SDM.Parsers.Common.Classes;

	internal static class ParserExtensions
	{
		public static OpenApiSchema? ToOpenApiSchema(this ParameterClass parameter, ControllerUnit unit)
		{
			var typeSymbol = unit.SemanticModel.GetTypeInfo(parameter.OriginalSyntaxNode.Type!).Type as INamedTypeSymbol;
			return typeSymbol?.ToOpenApiSchema(parameter.DefaultValue, unit);
		}

		public static OpenApiSchema? ToOpenApiSchema(this INamedTypeSymbol type, Value? defaultValue, ControllerUnit unit)
		{
			var schema = ComponentFactory.Create(type, unit.Compilation);
			if (schema is null)
			{
				return schema;
			}

			if (schema.Type == JsonSchemaType.Object)
			{
				schema.Items = new OpenApiSchemaReference(type.Name);
				return schema;
			}

			if (schema.Type == JsonSchemaType.Array)
			{
				if (type is IArrayTypeSymbol arrayType)
				{
					schema.Items = new OpenApiSchemaReference(arrayType.ElementType.Name);
					return schema;
				}
				else if (type is INamedTypeSymbol namedType &&
					(SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, TypeHelper.Instance.IEnumerableSymbol) ||
					namedType.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, TypeHelper.Instance.IEnumerableSymbol))))
				{
					schema.Items = new OpenApiSchemaReference(namedType.TypeArguments[0].Name);
					return schema;
				}
				else
				{
					throw new NotSupportedException($"Not supported list type: '{type.Name}'");
				}
			}

			return new OpenApiSchema
			{
				Type = type.SpecialType switch
				{
					SpecialType.System_Boolean => JsonSchemaType.Boolean,
					SpecialType.System_Char => JsonSchemaType.String,
					SpecialType.System_String => JsonSchemaType.String,
					SpecialType.System_DateTime => JsonSchemaType.String,
					SpecialType.System_Byte => JsonSchemaType.Integer,
					SpecialType.System_SByte => JsonSchemaType.Integer,
					SpecialType.System_Int16 => JsonSchemaType.Integer,
					SpecialType.System_UInt16 => JsonSchemaType.Integer,
					SpecialType.System_Int32 => JsonSchemaType.Integer,
					SpecialType.System_UInt32 => JsonSchemaType.Integer,
					SpecialType.System_Int64 => JsonSchemaType.Integer,
					SpecialType.System_UInt64 => JsonSchemaType.Integer,
					SpecialType.System_Double => JsonSchemaType.Number,
					SpecialType.System_Single => JsonSchemaType.Number,
					SpecialType.System_Decimal => JsonSchemaType.Number,
					SpecialType.System_Array => JsonSchemaType.Array,
					SpecialType.System_Object => JsonSchemaType.Object,
					_ => JsonSchemaType.Object,
				},

				Format = type.SpecialType switch
				{
					SpecialType.System_Byte => "int32",
					SpecialType.System_SByte => "int32",
					SpecialType.System_Int16 => "int32",
					SpecialType.System_UInt16 => "int32",
					SpecialType.System_Int32 => "int32",
					SpecialType.System_UInt32 => "int32",
					SpecialType.System_Int64 => "int64",
					SpecialType.System_UInt64 => "int64",
					SpecialType.System_Double => "double",
					SpecialType.System_Single => "float",
					SpecialType.System_Decimal => "double",
					SpecialType.System_DateTime => "date-time",
					_ => null,
				},

				Default = defaultValue.ToJsonNode(),
			};
		}

		public static JsonNode? ToJsonNode(this Value? value)
		{
			if (value == null)
				return null;

			// Handle arrays
			if (value.Array != null)
			{
				var arrayNode = new JsonArray();
				foreach (var item in value.Array)
				{
					arrayNode.Add(ToJsonNode(item));
				}

				return arrayNode;
			}

			// Handle primitive types and strings
			switch (value.Type?.ValueTypeKind)
			{
				case TypeClass.ValueType.Boolean:
					return JsonValue.Create(bool.TryParse(Convert.ToString(value.Object), out var b) ? b : (bool?)null);
				case TypeClass.ValueType.Char:
				case TypeClass.ValueType.String:
					return JsonValue.Create(value.AsString);
				case TypeClass.ValueType.Int8:
				case TypeClass.ValueType.UInt8:
				case TypeClass.ValueType.Int16:
				case TypeClass.ValueType.UInt16:
				case TypeClass.ValueType.Int32:
				case TypeClass.ValueType.UInt32:
				case TypeClass.ValueType.Int64:
				case TypeClass.ValueType.UInt64:
					if (long.TryParse(Convert.ToString(value.Object), out var l))
						return JsonValue.Create(l);
					break;
				case TypeClass.ValueType.Single:
				case TypeClass.ValueType.Double:
				case TypeClass.ValueType.Decimal:
					if (double.TryParse(Convert.ToString(value.Object), out var d))
						return JsonValue.Create(d);
					break;
				default:
					// Fallback: treat as string
					return JsonValue.Create(value.AsString);
			}

			return null;
		}

		public static int GetStatusCode(this InvocationExpressionSyntax methodCall, ControllerUnit unit)
		{
			var methodName = (methodCall.Expression as IdentifierNameSyntax)?.Identifier.Text ?? String.Empty;
			switch (methodName)
			{
				case "Ok":
					return 200;

				case "NotFound":
					return 404;

				case "BadRequest":
					return 400;

				case "Unauthorized":
					return 401;

				case "StatusCode":
				{
					var statusArg = methodCall.ArgumentList.Arguments[0].Expression;
					var constValue = unit.SemanticModel.GetConstantValue(statusArg);
					if (constValue.HasValue && constValue.Value is int statusCode)
					{
						return statusCode;
					}

					break;
				}

				default:
					return -1;
			}

			return -1;
		}

		public static ICollection<IPropertySymbol> GetProperties(this ITypeSymbol symbol, bool includeBase = true)
		{
			var properties = new List<IPropertySymbol>();
			if (symbol is null)
			{
				return properties;
			}

			properties.AddRange(symbol.GetMembers().OfType<IPropertySymbol>());

			if (!includeBase)
			{
				return properties;
			}

			if (symbol.BaseType is not null)
			{
				properties.AddRange(symbol.BaseType.GetProperties(includeBase));
			}

			return properties;
		}
	}
}
