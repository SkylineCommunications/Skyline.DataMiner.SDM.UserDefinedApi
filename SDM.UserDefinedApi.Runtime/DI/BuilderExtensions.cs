namespace Skyline.DataMiner.SDM.UserDefinedApi
{
	using System;
	using System.Linq;
	using System.Reflection;

	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.UserDefinedApi.DI;

	public static class BuilderExtensions
	{
		public static UserDefinedApi.UserDefinedApiBuilder AddControllersFromAssembly(
			this UserDefinedApi.UserDefinedApiBuilder builder,
			Assembly assembly)
		{
			if (builder == null)
			{
				throw new ArgumentNullException(nameof(builder));
			}

			// Find all public, non-abstract classes that inherit ControllerBase
			var controllerTypes = assembly.GetTypes()
			.Where(t => t.IsClass &&
						!t.IsAbstract &&
						typeof(ControllerBase).IsAssignableFrom(t) &&
						t.GetCustomAttribute<RouteAttribute>() != null);

			foreach (var controllerType in controllerTypes)
			{
				builder.AddController(controllerType);
			}

			return builder;
		}

		public static UserDefinedApi.UserDefinedApiBuilder AddControllers(
			this UserDefinedApi.UserDefinedApiBuilder builder)
		{
			if (builder == null)
			{
				throw new ArgumentNullException(nameof(builder));
			}

			var currentAssembly = Assembly.GetCallingAssembly();
			return builder.AddControllersFromAssembly(currentAssembly);
		}

		public static UserDefinedApi.UserDefinedApiBuilder AddRepository<TModel, TImplementation>(
			this UserDefinedApi.UserDefinedApiBuilder builder)
			where TModel : class
			where TImplementation : class, IRepositoryMarker<TModel>
		{
			return builder.ConfigureServices(
				services =>
				{
					services.AddRepository<TModel, TImplementation>();
				});
		}

		public static UserDefinedApi.UserDefinedApiBuilder AddRepository<TModel, TService, TImplementation>(
			this UserDefinedApi.UserDefinedApiBuilder builder)
			where TModel : class
			where TService : class, IRepositoryMarker<TModel>
			where TImplementation : class, TService
		{
			return builder.ConfigureServices(
				services =>
				{
					services.AddRepository<TModel, TService, TImplementation>();
				});
		}

		public static UserDefinedApi.UserDefinedApiBuilder AddRepository<TModel, TService>(
			this UserDefinedApi.UserDefinedApiBuilder builder,
			Func<IServiceProvider, TService> factory)
			where TModel : class
			where TService : class, IRepositoryMarker<TModel>
		{
			return builder.ConfigureServices(
				services =>
				{
					services.AddRepository<TModel, TService>(factory);
				});
		}
	}
}
