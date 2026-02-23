namespace Skyline.DataMiner.SDM.UserDefinedApi.OpenApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.Json.Nodes;

	using Microsoft.CodeAnalysis;
	using Microsoft.OpenApi;

	internal class ComponentFactory
	{
		private static Dictionary<SpecialType, OpenApiSchema> _handlers = new Dictionary<SpecialType, OpenApiSchema>
		{
			[SpecialType.System_String] = new OpenApiSchema { Type = JsonSchemaType.String },
			[SpecialType.System_Boolean] = new OpenApiSchema { Type = JsonSchemaType.Boolean },
			[SpecialType.System_Byte] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
			[SpecialType.System_SByte] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
			[SpecialType.System_Int16] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
			[SpecialType.System_UInt16] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
			[SpecialType.System_Int32] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
			[SpecialType.System_UInt32] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
			[SpecialType.System_Int64] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int64" },
			[SpecialType.System_UInt64] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int64" },
			[SpecialType.System_Double] = new OpenApiSchema { Type = JsonSchemaType.Number, Format = "double" },
			[SpecialType.System_Single] = new OpenApiSchema { Type = JsonSchemaType.Number, Format = "float" },
			[SpecialType.System_DateTime] = new OpenApiSchema { Type = JsonSchemaType.String, Format = "date-time" },
		};

		internal static OpenApiSchema? Create(INamedTypeSymbol? symbol, Compilation compilation)
		{
			if (symbol == null)
			{
				return null;
			}

			if (_handlers.TryGetValue(symbol.SpecialType, out var predefinedSchema))
			{
				return predefinedSchema;
			}

			if (TryCreateTimeSpanScheme(symbol, out var timeSpanScheme))
			{
				return timeSpanScheme;
			}

			if (TryCreateEnumScheme(symbol, out var enumSchema))
			{
				return enumSchema;
			}

			if (TryCreateGuidScheme(symbol, out var guidSchema))
			{
				return guidSchema;
			}

			if (TryCreateSdmObjectReferenceScheme(symbol, out var sdmObjectRefSchema))
			{
				return sdmObjectRefSchema;
			}

			if (TryCreateIEnumerableScheme(symbol, compilation, out var enumerableSchema))
			{
				return enumerableSchema;
			}

			if (TryCreateComplexScheme(symbol, compilation, out var complexSchema))
			{
				return complexSchema;
			}

			return null;
		}

		private static bool TryCreateEnumScheme(INamedTypeSymbol symbol, out OpenApiSchema? schema)
		{
			if (symbol.TypeKind != TypeKind.Enum)
			{
				schema = null;
				return false;
			}

			schema = new OpenApiSchema
			{
				Type = JsonSchemaType.String,
			};

			var enumValues = symbol.GetMembers()
				.Where(m => m.Kind == SymbolKind.Field && m is IFieldSymbol fs && fs.HasConstantValue)
				.Select(m => (JsonNode)JsonValue.Create(m.Name))
				.ToList();
			schema.Enum = enumValues;
			return true;
		}

		private static bool TryCreateTimeSpanScheme(INamedTypeSymbol symbol, out OpenApiSchema? schema)
		{
			if (SymbolEqualityComparer.Default.Equals(symbol, TypeHelper.Instance.TimeSpanSymbol))
			{
				schema = new OpenApiSchema
				{
					Type = JsonSchemaType.String,
					Description = "TimeSpan formatted as hh:mm:ss.fffffff",
					Example = JsonValue.Create("00:00:00.0000000"),
				};
				return true;
			}

			schema = null;
			return false;
		}

		private static bool TryCreateGuidScheme(INamedTypeSymbol symbol, out OpenApiSchema? schema)
		{
			if (SymbolEqualityComparer.Default.Equals(symbol, TypeHelper.Instance.GuidSymbol))
			{
				schema = new OpenApiSchema
				{
					Type = JsonSchemaType.String,
					Format = "uuid",
				};
				return true;
			}

			schema = null;
			return false;
		}

		private static bool TryCreateSdmObjectReferenceScheme(INamedTypeSymbol symbol, out OpenApiSchema? schema)
		{
			if (!SymbolEqualityComparer.Default.Equals(symbol.ConstructedFrom, TypeHelper.Instance.SdmObjectReferenceSymbol))
			{
				schema = null;
				return false;
			}

			var referencedType = symbol.TypeArguments[0] as INamedTypeSymbol;
			if (referencedType is null)
			{
				schema = null;
				return false;
			}

			if (TypeHelper.HasDomStorageAttribute(referencedType))
			{
				schema = new OpenApiSchema
				{
					Type = JsonSchemaType.String,
					Format = "uuid",
				};
				return true;
			}

			schema = _handlers[SpecialType.System_String];
			return true;
		}

		private static bool TryCreateIEnumerableScheme(ITypeSymbol symbol, Compilation compilation, out OpenApiSchema? schema)
		{
			// Check if it's an array
			if (symbol is IArrayTypeSymbol arrayType)
			{
				schema = new OpenApiSchema
				{
					Type = JsonSchemaType.Array,
					Items = Create(arrayType.ElementType as INamedTypeSymbol, compilation),
				};
				return true;
			}

			// Check if it implements IEnumerable<T>
			if (symbol is INamedTypeSymbol namedSymbol &&
				(SymbolEqualityComparer.Default.Equals(namedSymbol.OriginalDefinition, TypeHelper.Instance.IEnumerableSymbol) ||
				namedSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, TypeHelper.Instance.IEnumerableSymbol))))
			{
				schema = new OpenApiSchema
				{
					Type = JsonSchemaType.Array,
					Items = Create(namedSymbol.TypeArguments[0] as INamedTypeSymbol, compilation),
				};
				return true;
			}

			schema = null;
			return false;
		}

		private static bool TryCreateComplexScheme(ITypeSymbol symbol, Compilation compilation, out OpenApiSchema? schema)
		{
			if (symbol.TypeKind != TypeKind.Class)
			{
				schema = null;
				return false;
			}

			schema = new OpenApiSchema
			{
				Type = JsonSchemaType.Object,
			};

			schema.Properties = new Dictionary<string, IOpenApiSchema>();
			foreach (var member in symbol.GetProperties().Where(p => p.DeclaredAccessibility == Accessibility.Public))
			{
				var create = Create(member.Type as INamedTypeSymbol, compilation);
				////if (member.Type is INamedTypeSymbol propType &&
				////	SymbolEqualityComparer.Default.Equals(propType.ConstructedFrom, TypeHelper.Instance.SdmObjectReferenceSymbol))
				////{
				////	create = Create(TypeHelper.Instance.GuidSymbol!, compilation);
				////}
				////else
				////{
				////	create = Create(member.Type as INamedTypeSymbol, compilation);
				////}

				if (create is null)
				{
					continue;
				}

				schema.Properties[member.Name] = create;
			}

			return true;
		}
	}
}
