namespace Skyline.DataMiner.SDM.UserDefinedApi.OpenApi
{
	using System;
	using System.Linq;

	using Microsoft.CodeAnalysis;

	using Skyline.DataMiner.SDM.Parsers.Common.Classes;

	internal class ControllerUnit
	{
		internal ControllerUnit(ClassClass @class, SemanticModel semanticModel, Compilation compilation)
		{
			Class = @class ?? throw new ArgumentNullException(nameof(@class));
			SemanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
			Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
		}

		public ClassClass Class { get; }

		public SemanticModel SemanticModel { get; }

		public Compilation Compilation { get; }

		public string GetRoute()
		{
			var attribute = Class.Attributes.FirstOrDefault(a => a.Name == "Route" || a.Name == "RouteAttribute");
			if(attribute is null)
			{
				return "/";
			}

			var constant = SemanticModel.GetConstantValue(attribute.Arguments[0].OriginalSyntaxNode.Expression);
			if(constant.HasValue && constant.Value is string route)
			{
				return route;
			}

			return "/";
		}
	}
}
