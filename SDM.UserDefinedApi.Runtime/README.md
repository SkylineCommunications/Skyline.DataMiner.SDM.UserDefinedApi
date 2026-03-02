# Skyline.DataMiner.SDM.UserDefinedApi.Runtime

## About

Runtime components for building User-Defined APIs in DataMiner. This package contains the core classes, attributes, and infrastructure needed to handle API requests, routing, and responses at runtime.

> **Note**: This is the runtime package. For the complete SDK experience including automatic OpenAPI generation and packaging, use the main `Skyline.DataMiner.SDM.UserDefinedApi` package instead.

## What's Included

### Core Components

- **ControllerBase**: Base class for API controllers with built-in response helpers
- **UserDefinedApi**: Main entry point for processing API requests
- **ApiController & Route Attributes**: Familiar ASP.NET Core-style routing decorators
- **HTTP Method Attributes**: `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`, etc.

### Dependency Injection

- **IServiceCollection Extensions**: Configure services with familiar DI patterns
- **IAccessor<T>**: Access DataMiner engine and request context
- Built-in service resolution in controllers

### Request/Response Handling

- **Parameter Binding**: `[FromBody]`, `[FromQuery]` attributes
- **Response Types**: `IActionResult`, `ObjectResult`, `StatusCodeResult`
- **Content Negotiation**: Automatic JSON serialization with configurable formatters

### Data Querying

- **OData Support**: Built-in OData query parsing and filtering
- **Filter Translation**: Convert OData filters to DataMiner filter elements
- **Query Operators**: Support for `eq`, `ne`, `gt`, `lt`, `and`, `or`, etc.

## Usage Examples

### Creating a Controller

```csharp
using Skyline.DataMiner.SDM.UserDefinedApi;

[ApiController]
[Route("api/resources")]
public class ResourceController : ControllerBase
{
    private readonly IEngine _engine;

    public ResourceController(IAccessor<IEngine> engineAccessor)
    {
        _engine = engineAccessor.Value;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var resources = FetchResources();
        return Ok(resources);
        }

    [HttpGet]
    public IActionResult GetById(string id)
    {
        var resource = FetchResourceById(id);
        if (resource == null)
            return NotFound();

        return Ok(resource);
    }

    [HttpPost]
    public IActionResult Create([FromBody] Resource resource)
    {
        if (resource == null)
            return BadRequest("Resource cannot be null");

        var created = CreateResource(resource);
        return StatusCode(201, created);
    }
}
```

### Handling API Requests

```csharp
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.SDM.UserDefinedApi;

public class Script
{
    private static IUserDefinedApi _api;

    [AutomationEntryPoint(AutomationEntryPointType.Types.OnApiTrigger)]
    public ApiTriggerOutput OnApiTrigger(IEngine engine, ApiTriggerInput requestData)
    {
        if(_api is null)
        {
            var builder = UserDefinedApi.CreateBuilder();

            // Optional: Configure services
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IMyService, MyService>();
            });

            _api = builder.Build();
        }

        return _api.Run(engine, requestData);
    }
}
```

### Using OData Filtering

```csharp
[HttpGet]
public IActionResult GetTickets([FromQuery] string filter)
{
    var translator = new ODataSdmTranslator<Ticket>();
    var filter = translator.TranslateFilter(filter);
    var tickets = _ticketRepository.Read(filter);

    // OData filter examples:
    // $filter=Severity eq 'High'

    return Ok(tickets);
}
```

### Dependency Injection

```csharp
public class MyController : ControllerBase
{
    private readonly IEngine _engine;
    private readonly ILogger<MyController> _logger;
    private readonly IMyService _myService;

    public MyController(
        IAccessor<IEngine> engineAccessor,
        ILogger<MyController> logger,
        IMyService myService)
    {
        _engine = engineAccessor.Value;
        _logger = logger;
        _myService = myService;
    }
}
```

### Response Helpers

```csharp
// Success responses
return Ok(data);                           // 200 OK
return StatusCode(201, created);           // 201 Created with body
return StatusCode(204);                    // 204 No Content

// Error responses
return BadRequest("Invalid input");         // 400 Bad Request
return NotFound();                          // 404 Not Found
return StatusCode(500, "Server error");     // Custom status code
```

## Related Packages

- **Skyline.DataMiner.SDM.UserDefinedApi**: Complete SDK with build-time tooling
- **Skyline.DataMiner.Dev.Utils.SDM.Abstractions**: Common abstractions and interfaces

## About DataMiner

DataMiner is a transformational platform that provides vendor-independent control and monitoring of devices and services. Out of the box and by design, it addresses key challenges such as security, complexity, multi-cloud, and much more. It has a pronounced open architecture and powerful capabilities enabling users to evolve easily and continuously.

The foundation of DataMiner is its powerful and versatile data acquisition and control layer. With DataMiner, there are no restrictions to what data users can access. Data sources may reside on premises, in the cloud, or in a hybrid setup.

A unique catalog of 7000+ connectors already exists. In addition, you can leverage DataMiner Development Packages to build your own connectors (also known as "protocols" or "drivers").

> **Note**
> See also: [About DataMiner](https://aka.dataminer.services/about-dataminer).

## About Skyline Communications

At Skyline Communications, we deal in world-class solutions that are deployed by leading companies around the globe. Check out [our proven track record](https://aka.dataminer.services/about-skyline) and see how we make our customers' lives easier by empowering them to take their operations to the next level.
