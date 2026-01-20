namespace Skyline.DataMiner.SDM.UserDefinedApi.OpenApi
{
	using Microsoft.OpenApi;

	internal class UserDefinedApiSecurityRequirement : OpenApiSecuritySchemeReference
	{
		private readonly string _referenceId;
		private readonly OpenApiDocument _doc;

		public UserDefinedApiSecurityRequirement(string referenceId, OpenApiDocument hostDoc)
			: base(referenceId, hostDoc)
		{
			_referenceId = referenceId;
			_doc = hostDoc;
		}

		public override IOpenApiSecurityScheme? Target
		{
			get
			{
				if (_doc is null)
				{
					return default;
				}

				if(_doc.Components?.SecuritySchemes is null)
				{
					return default;
				}

				return _doc.Components.SecuritySchemes.TryGetValue(_referenceId, out var scheme) ? scheme : default;
			}
		}
	}
}
