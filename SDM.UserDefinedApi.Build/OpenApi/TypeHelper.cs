namespace Skyline.DataMiner.SDM.UserDefinedApi.OpenApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp.Syntax;

	internal class TypeHelper
	{
		private static TypeHelper _instance = new TypeHelper();

		private readonly INamedTypeSymbol? _ienumerableSymbol;
		private readonly INamedTypeSymbol? _iapiResultSymbol;
		private readonly INamedTypeSymbol? _sdmObjectRefSymbol;
		private readonly INamedTypeSymbol? _sdmDomStorageAttributeSymbol;
		private readonly INamedTypeSymbol? _guidSymbol;

		private TypeHelper()
		{
		}

		private TypeHelper(Compilation compilation)
		{
			_ienumerableSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1") ?? throw new InvalidOperationException("Could not find IEnumerable<T> symbol.");
			_iapiResultSymbol = compilation.GetTypeByMetadataName("Skyline.DataMiner.SDM.UserDefinedApi.IApiResult`1") ?? throw new InvalidOperationException("Could not find IApiResult<T> symbol.");
			_sdmObjectRefSymbol = compilation.GetTypeByMetadataName("Skyline.DataMiner.SDM.SdmObjectReference`1");
			_sdmDomStorageAttributeSymbol = compilation.GetTypeByMetadataName("Skyline.DataMiner.SDM.SdmDomStorageAttribute");
			_guidSymbol = compilation.GetTypeByMetadataName("System.Guid");
		}

		public static TypeHelper Instance => _instance;

		public INamedTypeSymbol? IEnumerableSymbol { get => _ienumerableSymbol; }

		public INamedTypeSymbol? IApiResultSymbol { get => _iapiResultSymbol; }

		public INamedTypeSymbol? SdmObjectReferenceSymbol { get => _sdmObjectRefSymbol; }

		public INamedTypeSymbol? SdmDomStorageAttributeSymbol { get => _sdmDomStorageAttributeSymbol; }

		public INamedTypeSymbol? GuidSymbol { get => _guidSymbol; }

		public static void Load(Compilation compilation)
		{
			_instance = new TypeHelper(compilation);
		}

		/// <summary>
		/// Gets the element type of an array or a generic enumerable type, such as IEnumerable<T> or arrays. If the specified
		/// type is not a collection, returns the type itself.
		/// </summary>
		/// <param name="type">The type symbol to inspect for an element type. This can be an array, a generic enumerable type, or any other
		/// type.</param>
		/// <param name="compilation">The compilation context used to resolve type symbols, such as System.Collections.Generic.IEnumerable<T>.</param>
		/// <returns>The element type if the specified type is an array or implements IEnumerable<T>; otherwise, the original type
		/// symbol.</returns>
		/// <remarks>This method supports both array types and types that implement the generic IEnumerable<T>
		/// interface. For non-collection types, the method returns the type itself.</remarks>
		public static INamedTypeSymbol? GetElementType(ITypeSymbol type, Compilation compilation)
		{
			if (type is IArrayTypeSymbol arrayType)
			{
				// Array
				return arrayType.ElementType as INamedTypeSymbol;
			}

			// Check if IEnumerable<T>
			var enumerableType = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
			if (type is INamedTypeSymbol namedType && SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, enumerableType))
			{
				return namedType.TypeArguments[0] as INamedTypeSymbol;
			}

			// Check for IEnumerable<T>
			var ienumerable = type.AllInterfaces
				.FirstOrDefault(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, enumerableType));

			if (ienumerable != null)
			{
				return ienumerable.TypeArguments[0] as INamedTypeSymbol;
			}

			// fallback: not a collection
			return type as INamedTypeSymbol;
		}

		/// <summary>
		/// Determines the API result type returned by a specified return statement within a controller unit.
		/// </summary>
		/// <remarks>This method analyzes the expression in the return statement and resolves its type using the
		/// provided semantic model. If the expression or its type is unavailable, or if the API result type cannot be
		/// resolved, the method returns <see langword="null"/>.</remarks>
		/// <param name="unit">The controller unit containing the semantic model and compilation context used to analyze the return statement.</param>
		/// <param name="statement">The return statement syntax node for which to determine the return type.</param>
		/// <returns>An <see cref="ITypeSymbol"/> representing the API result type of the return statement, or <see langword="null"/>
		/// if the type cannot be determined.</returns>
		public static INamedTypeSymbol? GetReturnType(ControllerUnit unit, ReturnStatementSyntax statement)
		{
			var expression = statement.Expression;
			if (expression is null)
			{
				return null;
			}

			var payloadType = unit.SemanticModel.GetTypeInfo(expression).Type;
			if (payloadType is null)
			{
				return null;
			}

			var apiResultSymbol = payloadType.AllInterfaces
				.FirstOrDefault(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, Instance.IApiResultSymbol));
			if (apiResultSymbol is null)
			{
				return null;
			}

			return apiResultSymbol.TypeArguments[0] as INamedTypeSymbol;
		}

		public static bool HasDomStorageAttribute(ITypeSymbol symbol)
		{
			return symbol.GetAttributes().Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, Instance.SdmDomStorageAttributeSymbol));
		}
	}
}
