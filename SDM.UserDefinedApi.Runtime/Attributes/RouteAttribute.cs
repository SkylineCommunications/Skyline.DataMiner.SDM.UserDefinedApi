namespace Skyline.DataMiner.SDM.UserDefinedApi
{
	using System;

	/// <summary>
	/// Specifies a route template for a user-defined API class.
	/// </summary>
	/// <remarks>
	/// This attribute is used to define the route template that will be associated with a class.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class RouteAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RouteAttribute"/> class with the specified route template.
		/// </summary>
		/// <param name="template">The route template to associate with the class without the 'api/custom' prefix.</param>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="template"/> is <c>null</c> or an empty string.
		/// </exception>
		public RouteAttribute(string template)
		{
			if (String.IsNullOrEmpty(template))
			{
				throw new ArgumentNullException(nameof(template));
			}

			Template = template;
		}

		/// <summary>
		/// Gets the route template associated with the class.
		/// </summary>
		public string Template { get; }
	}
}
