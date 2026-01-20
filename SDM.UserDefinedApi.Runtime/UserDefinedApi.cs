namespace Skyline.DataMiner.SDM.UserDefinedApi
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Reflection;

	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.SDM.UserDefinedApi.DI;
	using Skyline.DataMiner.SDM.UserDefinedApi.Formatters;

	using SLLoggerUtil;

	/// <summary>
	/// Represents a user-defined API that can route requests to registered controllers.
	/// </summary>
	public class UserDefinedApi : IUserDefinedApi
	{
		private readonly IServiceProvider _rootProvider;
		private readonly List<RouteHandlerInfo> _handlers;

		/// <summary>
		/// Initializes a new instance of the <see cref="UserDefinedApi"/> class.
		/// </summary>
		/// <param name="handlers">The list of route handler information.</param>
		/// <param name="rootProvider">The root service provider.</param>
		private UserDefinedApi(
			List<RouteHandlerInfo> handlers,
			IServiceProvider rootProvider)
		{
			_handlers = handlers;
			_rootProvider = rootProvider;
		}

		/// <inheritdoc/>
		public IInputConverter DefaultInputConverter { get => InputConverters[0]; }

		/// <inheritdoc/>
		public IOutputConverter DefaultOutputConverter { get => OutputConverters[0]; }

		/// <inheritdoc/>
		public IReadOnlyList<IInputConverter> InputConverters { get; private set; } = new ReadOnlyCollection<IInputConverter>(new List<IInputConverter>());

		/// <inheritdoc/>
		public IReadOnlyList<IOutputConverter> OutputConverters { get; private set; } = new ReadOnlyCollection<IOutputConverter>(new List<IOutputConverter>());

		/// <summary>
		/// Creates a new <see cref="UserDefinedApiBuilder"/> instance for building a <see cref="UserDefinedApi"/>.
		/// </summary>
		/// <returns>A new <see cref="UserDefinedApiBuilder"/> instance.</returns>
		public static UserDefinedApiBuilder CreateBuilder()
		{
			return new UserDefinedApiBuilder();
		}

		/// <inheritdoc />
		public ApiTriggerOutput Run(IEngine engine, ApiTriggerInput apiTriggerInput)
		{
			using (var scope = _rootProvider.CreateScope())
			{
				scope.ServiceProvider
					.GetRequiredService<IAccessor<IEngine>>()
					.SetValue(engine);

				var apiContext = new ApiContext
				{
					Request = apiTriggerInput,
					Response = new ApiTriggerOutput(),
					DefaultInputConverter = DefaultInputConverter,
					DefaultOutputConverter = DefaultOutputConverter,
					InputConverters = InputConverters,
					OutputConverters = OutputConverters,
				};

				var routeHandler = _handlers
					.Select(route => new { Route = route, Rank = route.GetRank(apiContext) })
					.Where(a => a.Rank >= 0)
					.OrderByDescending(a => a.Rank)
					.Select(a => a.Route)
					.FirstOrDefault();

				if (routeHandler is null)
				{
					throw new InvalidOperationException($"Could not find a matching route handler for route '{apiContext.Request.RequestMethod} {apiContext.Request.Route}'");
				}

				var controller = (ControllerBase)scope.ServiceProvider
					.GetRequiredService(routeHandler.ControllerType);
				controller.ApiContext = apiContext;

				var result = routeHandler.Invoke(apiContext, controller, _rootProvider);
				if (result is null)
				{
					throw new InvalidOperationException("The API action returned a null result.");
				}

				result.ExecuteResult(apiContext);
				return apiContext.Response;
			}
		}

		/// <summary>
		/// Sets the input converters for the API.
		/// </summary>
		/// <param name="converters">The collection of input converters to set.</param>
		private void SetInputConverters(IEnumerable<IInputConverter> converters)
		{
			InputConverters = new ReadOnlyCollection<IInputConverter>(converters.ToList());
		}

		/// <summary>
		/// Sets the output converters for the API.
		/// </summary>
		/// <param name="converters">The collection of output converters to set.</param>
		private void SetOutputConverters(IEnumerable<IOutputConverter> converters)
		{
			OutputConverters = new ReadOnlyCollection<IOutputConverter>(converters.ToList());
		}

		/// <summary>
		/// Validates that all route handler parameters can be converted by the registered input converters.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// Thrown when one or more handler parameters cannot be converted by any registered input converter.
		/// </exception>
		private void Validate()
		{
			// Check if all the input arguments can be deserialized.
			var unhandledParameters = new List<ParameterInfo>();
			foreach (var handler in _handlers)
			{
				unhandledParameters.AddRange(handler.MethodParameters.Where(p =>
				{
					return !InputConverters.Reverse().Any(c => c.CanConvertInput(p.ParameterType));
				}));
			}

			if (unhandledParameters.DistinctBy(p => p.ParameterType.FullName).Any())
			{
				throw new InvalidOperationException($"No input converter found for the following parameters of types '{unhandledParameters.Select(p => p.ParameterType.FullName)}'");
			}
		}

		/// <summary>
		/// Builder class for constructing <see cref="UserDefinedApi"/> instances.
		/// </summary>
		public class UserDefinedApiBuilder
		{
			private readonly List<RouteHandlerInfo> _handlers = new();
			private readonly List<Action<IServiceCollection>> _configureActions = new List<Action<IServiceCollection>>();

			private readonly HashSet<IInputConverter> _inputConverters = new();
			private readonly HashSet<IOutputConverter> _outputConverters = new();

			private readonly IServiceCollection _services;
			private IInputConverter _inputConverter = new NewtonsoftFormatter();
			private IOutputConverter _outputConverter = new NewtonsoftFormatter();

			/// <summary>
			/// Initializes a new instance of the <see cref="UserDefinedApiBuilder"/> class.
			/// </summary>
			internal UserDefinedApiBuilder()
			{
				_services = new ServiceCollection();
			}

			/// <summary>
			/// Adds a controller of type <typeparamref name="T"/> to the API.
			/// </summary>
			/// <typeparam name="T">The type of controller to add. Must inherit from <see cref="ControllerBase"/>.</typeparam>
			/// <returns>The current <see cref="UserDefinedApiBuilder"/> instance for method chaining.</returns>
			public UserDefinedApiBuilder AddController<T>()
				where T : ControllerBase
			{
				var controllerType = typeof(T);
				return AddController(controllerType);
			}

			/// <summary>
			/// Adds a controller of the specified type to the API.
			/// </summary>
			/// <param name="controllerType">The type of controller to add.</param>
			/// <returns>The current <see cref="UserDefinedApiBuilder"/> instance for method chaining.</returns>
			/// <exception cref="InvalidOperationException">
			/// Thrown when the controller does not have a valid <see cref="RouteAttribute"/>.
			/// </exception>
			public UserDefinedApiBuilder AddController(Type controllerType)
			{
				var controllerRoute = controllerType
					.GetCustomAttribute<RouteAttribute>()?
					.Template?
					.Trim('/') ?? String.Empty;

				if (String.IsNullOrEmpty(controllerRoute))
				{
					throw new InvalidOperationException($"Controller '{controllerType}' does not have a valid Route attribute.");
				}

				_services.AddScoped(controllerType);

				var methods = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
				foreach (var method in methods)
				{
					var httpMethodAttr = method.GetCustomAttribute<HttpMethodAttribute>(true);
					if (httpMethodAttr is null)
					{
						continue;
					}

					var parameters = method.GetParameters();
					var routeInfo = new RouteHandlerInfo(
						controllerType,
						httpMethodAttr.HttpMethod,
						controllerRoute,
						method,
						parameters);

					_handlers.Add(routeInfo);
				}

				return this;
			}

			/// <summary>
			/// Adds a configuration action to be applied to the service collection during API setup.
			/// </summary>
			/// <param name="configure">An action that configures the <see cref="IServiceCollection"/>. Cannot be null.</param>
			/// <returns>The current <see cref="UserDefinedApiBuilder"/> instance for method chaining.</returns>
			/// <exception cref="ArgumentNullException">Thrown if <paramref name="configure"/> is null.</exception>
			public UserDefinedApiBuilder ConfigureServices(Action<IServiceCollection> configure)
			{
				if (configure is null)
				{
					throw new ArgumentNullException(nameof(configure));
				}

				_configureActions.Add(configure);
				return this;
			}

			/// <summary>
			/// Sets the default input converter for the API.
			/// </summary>
			/// <param name="converter">The input converter to use by default.</param>
			/// <returns>The current <see cref="UserDefinedApiBuilder"/> instance for method chaining.</returns>
			public UserDefinedApiBuilder WithDefaultInputConverter(IInputConverter converter)
			{
				_inputConverter = converter;
				return this;
			}

			/// <summary>
			/// Sets the default output converter for the API.
			/// </summary>
			/// <param name="converter">The output converter to use by default.</param>
			/// <returns>The current <see cref="UserDefinedApiBuilder"/> instance for method chaining.</returns>
			public UserDefinedApiBuilder WithDefaultOutputConverter(IOutputConverter converter)
			{
				_outputConverter = converter;
				return this;
			}

			/// <summary>
			/// Adds an input converter to the API.
			/// </summary>
			/// <param name="converter">The input converter to add.</param>
			/// <returns>The current <see cref="UserDefinedApiBuilder"/> instance for method chaining.</returns>
			public UserDefinedApiBuilder AddInputConverter(IInputConverter converter)
			{
				_inputConverters.Add(converter);
				return this;
			}

			/// <summary>
			/// Adds an output converter to the API.
			/// </summary>
			/// <param name="converter">The output converter to add.</param>
			/// <returns>The current <see cref="UserDefinedApiBuilder"/> instance for method chaining.</returns>
			public UserDefinedApiBuilder AddOutputConverter(IOutputConverter converter)
			{
				_outputConverters.Add(converter);
				return this;
			}

			/// <summary>
			/// Builds the <see cref="UserDefinedApi"/> instance using the configured controllers and converters.
			/// </summary>
			/// <returns>A new <see cref="UserDefinedApi"/> instance.</returns>
			/// <exception cref="InvalidOperationException">
			/// Thrown when validation fails for input converters.
			/// </exception>
			public UserDefinedApi Build()
			{
				foreach (var action in _configureActions)
				{
					action(_services);
				}

				_services.AddScoped<IAccessor<IEngine>, EngineAccessor>();
				_services.AddScoped(sp => sp.GetRequiredService<IAccessor<IEngine>>().Value.GetUserConnection());
				_services.AddScoped(typeof(ILogger<>), typeof(EngineLogger<>));
				_services.AddScoped<Microsoft.Extensions.Logging.ILogger>(sp => new EngineLogger(sp.GetRequiredService<IAccessor<IEngine>>()));

				var api = new UserDefinedApi(
					_handlers,
					_services.BuildServiceProvider(
						new ServiceProviderOptions
						{
							ValidateScopes = true,
							ValidateOnBuild = true,
						}));

				var inputConverters = new List<IInputConverter> { _inputConverter };
				inputConverters.AddRange(_inputConverters);
				api.SetInputConverters(inputConverters);

				var outputConverters = new List<IOutputConverter> { _outputConverter };
				outputConverters.AddRange(_outputConverters);
				api.SetOutputConverters(outputConverters);

				api.Validate();

				return api;
			}
		}
	}
}
