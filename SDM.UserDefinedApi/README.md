# Skyline.DataMiner.SDM.UserDefinedApi

## About

A comprehensive SDK for building User-Defined APIs in DataMiner, providing a modern, ASP.NET Core-inspired development experience with automatic OpenAPI documentation generation and seamless DataMiner integration.

## Key Features

- **Controller-Based Architecture**: Build APIs using familiar ASP.NET Core patterns with `[ApiController]` and `[Route]` attributes
- **Automatic OpenAPI Generation**: Generate OpenAPI 3.0 specifications automatically from your code
- **OData Support**: Built-in OData filtering capabilities for querying collections
- **Type-Safe**: Leverage C# type system with full IntelliSense support
- **Dependency Injection**: Built-in DI container for clean, testable code
- **Automatic Packaging**: Creates ready-to-deploy `.dmapp` packages with install scripts
- **Multiple Response Formats**: Support for JSON serialization with configurable formatters

## Getting Started

### Installation

```bash
dotnet add package Skyline.DataMiner.SDM.UserDefinedApi
```

### Creating Your First API

1. **Create a Controller**

```csharp
using Skyline.DataMiner.SDM.UserDefinedApi;

[ApiController]
[Route("api/tickets")]
public class TicketController : ControllerBase
{
    [HttpGet]
    public IActionResult GetTickets()
    {
        var tickets = new[]
        {
            new Ticket { Id = 1, Title = "Sample Ticket", Status = TicketStatus.Open }
        };
        return Ok(tickets);
    }

    [HttpGet]
    public IActionResult GetTicket(int id)
    {
        var ticket = new Ticket { Id = id, Title = "Sample Ticket", Status = TicketStatus.Open };
        return Ok(ticket);
    }

    [HttpPost]
    public IActionResult CreateTicket([FromBody] Ticket ticket)
    {
        // Your creation logic here
        return Created($"api/tickets/{ticket.Id}", ticket);
    }
}

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; }
    public TicketStatus Status { get; set; }
}

public enum TicketStatus
{
    Open,
    InProgress,
    Closed
}
```

2. **Configure Your API Entry Point**

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
            _api = UserDefinedApi.CreateBuilder()
                .AddControllers()
                .Build();
        }

        return _api.Run(engine, requestData);
    }
}
```

### Build Configuration

The package can optionally automatically:
- Generates OpenAPI specification during build
- Creates installation packages (.dmapp files)
- Bundles all required dependencies

The build output includes:
- `{ProjectName}.dmapp` - Ready-to-install DataMiner package
- `openapi/openapi.yaml` - OpenAPI 3.0 specification

## Advanced Features

### OData Filtering

Controllers automatically support OData-style filtering:

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

Use built-in DI for cleaner architecture:

```csharp
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }
}

// Configure services
apiBuilder.ConfigureServices(services =>
{
    services.AddSingleton<ITicketService, TicketService>();
});
```

## OpenAPI Support

The SDK automatically generates OpenAPI documentation including:
- Route definitions
- HTTP methods (GET, POST, PUT, DELETE, etc.)
- Request/response schemas
- Enum values as strings
- Complex object schemas
- Array/collection types

Access your API documentation at the generated `openapi/openapi.yaml` file.

## About DataMiner

DataMiner is a transformational platform that provides vendor-independent control and monitoring of devices and services. Out of the box and by design, it addresses key challenges such as security, complexity, multi-cloud, and much more. It has a pronounced open architecture and powerful capabilities enabling users to evolve easily and continuously.

The foundation of DataMiner is its powerful and versatile data acquisition and control layer. With DataMiner, there are no restrictions to what data users can access. Data sources may reside on premises, in the cloud, or in a hybrid setup.

A unique catalog of 7000+ connectors already exists. In addition, you can leverage DataMiner Development Packages to build your own connectors (also known as "protocols" or "drivers").

> **Note**
> See also: [About DataMiner](https://aka.dataminer.services/about-dataminer).

## About Skyline Communications

At Skyline Communications, we deal in world-class solutions that are deployed by leading companies around the globe. Check out [our proven track record](https://aka.dataminer.services/about-skyline) and see how we make our customers' lives easier by empowering them to take their operations to the next level.
