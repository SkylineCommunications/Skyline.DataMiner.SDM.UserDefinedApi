namespace Skyline.DataMiner.SDM.UserDefinedApi.DI
{
	using System;
	using System.Linq;

	using Microsoft.Extensions.DependencyInjection;

	using Skyline.DataMiner.SDM;

	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRepository<TModel, TImplementation>(this IServiceCollection services)
			where TModel : class
			where TImplementation : class, IRepositoryMarker<TModel>
		{
			var markerType = typeof(IRepositoryMarker<>).MakeGenericType(typeof(TModel));
			var serviceTypes = typeof(TImplementation)
				.GetInterfaces()
				.Where(i =>
					i.IsGenericType &&
					markerType.IsAssignableFrom(i));

			foreach (var serviceType in serviceTypes)
			{
				services.AddScoped(serviceType, typeof(TImplementation));
			}

			services.AddScoped<TImplementation>();
			return services;
		}

		public static IServiceCollection AddRepository<TModel, TService, TImplementation>(this IServiceCollection services)
			where TModel : class
			where TService : class, IRepositoryMarker<TModel>
			where TImplementation : class, TService
		{
			var markerType = typeof(IRepositoryMarker<>).MakeGenericType(typeof(TModel));
			var serviceTypes = typeof(TService)
				.GetInterfaces()
				.Where(i =>
					i.IsGenericType &&
					markerType.IsAssignableFrom(i));

			foreach (var serviceType in serviceTypes)
			{
				services.AddScoped(serviceType, typeof(TImplementation));
			}

			services.AddScoped<TService, TImplementation>();
			return services;
		}

		public static IServiceCollection AddRepository<TModel, TService>(this IServiceCollection services, Func<IServiceProvider, TService> factory)
			where TModel : class
			where TService : class, IRepositoryMarker<TModel>
		{
			var markerType = typeof(IRepositoryMarker<>).MakeGenericType(typeof(TModel));
			var serviceTypes = typeof(TService)
				.GetInterfaces()
				.Where(i =>
					i.IsGenericType &&
					markerType.IsAssignableFrom(i));

			foreach (var serviceType in serviceTypes)
			{
				services.AddScoped(serviceType, factory);
			}

			services.AddScoped(factory);
			return services;
		}
	}
}
