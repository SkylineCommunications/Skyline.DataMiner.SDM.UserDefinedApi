# Skyline.DataMiner.SDM.UserDefinedApi

A framework for building User-Defined APIs in DataMiner, providing a controller-based approach similar to ASP.NET Core for creating RESTful APIs.

## About

This SDK simplifies the creation of User-Defined APIs in DataMiner by providing:

- **Controller-based architecture**: Define API endpoints using familiar controller classes
- **Attribute routing**: Use attributes like `[HttpGet]`, `[HttpPost]`, `[Route]` to define routes
- **Dependency injection**: Built-in DI container support for services and repositories
- **Automatic OpenAPI documentation**: Generate OpenAPI specifications from your controllers
- **OData support**: Query your data using OData conventions

## Getting Started

### Installation

Install the NuGet package:

```bash
Install-Package Skyline.DataMiner.SDM.UserDefinedApi
```

Or via .NET CLI:

```bash
dotnet add package Skyline.DataMiner.SDM.UserDefinedApi
```

### Basic Usage

#### 1. Create the API Entry Point

Create your automation script with the API trigger entry point:

```csharp
namespace Skyline.DataMiner.SDM.Registration.UDAPI
{
    using System.Runtime.CompilerServices;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
    using Skyline.DataMiner.SDM.UserDefinedApi;
    using Skyline.DataMiner.SDM.UserDefinedApi.DI;

    public static class Script
    {
        private static IUserDefinedApi _api;

        [AutomationEntryPoint(AutomationEntryPointType.Types.OnApiTrigger)]
        public static ApiTriggerOutput OnApiTrigger(IEngine engine, ApiTriggerInput requestData)
        {
            if (_api is null)
            {
                _api = UserDefinedApi.CreateBuilder()
                    .AddControllers()
                    .AddRepository<SolutionRegistration, SolutionRegistrationDomRepository>()
                    .Build();
            }

            return _api.Run(engine, requestData);
        }
    }
}
```

#### 2. Define a Controller

Create a controller class that inherits from `ControllerBase`:

```csharp
using Skyline.DataMiner.SDM.UserDefinedApi;

[ApiController]
[Route("api/solutions")]
public class SolutionsController : ControllerBase
{
    private readonly ISolutionRegistrationRepository _repository;

    public SolutionsController(ISolutionRegistrationRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var solutions = _repository.GetAll();
        return Ok(solutions);
    }

    [HttpPost]
    public IActionResult Create([FromBody] SolutionRegistration solution)
    {
        _repository.Create(solution);
        return StatusCode(201, solution);
    }

    public IActionResult Update([FromBody] SolutionRegistration solution)
    {
        var existing = _repository.GetById(id);
        if (existing == null)
        {
            return NotFound();
        }
        
        _repository.Update(solution);
        return Ok(solution);
    }

    public IActionResult Delete([FromQuery] string id)
    {
        var existing = _repository.GetById(id);
        if (existing == null)
        {
            return NotFound();
        }
        
        _repository.Delete(id);
        return Ok();
    }
}
```

#### 3. Register Dependencies

Use the builder pattern to register controllers and services:

```csharp
_api = UserDefinedApi.CreateBuilder()
    .AddControllers()  // Scans for controllers in the assembly
    .AddRepository<IMyRepository, MyRepository>()  // Register repositories
    .Build();
```

## Features

### HTTP Method Attributes

- `[HttpGet]` - Handle GET requests
- `[HttpPost]` - Handle POST requests
- `[HttpPut]` - Handle PUT requests
- `[HttpDelete]` - Handle DELETE requests

### Routing

Define routes using the `[Route]` attribute:

```csharp
[Route("api/users")]  // Controller-level route
public class UsersController : ControllerBase
{
    [HttpGet]  // GET api/users
    public IActionResult GetAll() { ... }

    [HttpGet]  // GET api/users?id={id}
    public IActionResult GetById([FromQuery] string id) { ... }

    [HttpPost]  // POST api/users
    public IActionResult Create([FromBody] User user) { ... }
}
```

### Parameter Binding

- `[FromBody]` - Bind from request body (automatic JSON deserialization)
- `[FromQuery]` - Bind from query string parameters
- Route parameters - Automatically bound from query parameters

### Response Types

Return different HTTP status codes and responses:

```csharp
// Return 200 OK
return Ok();
return Ok(data);

// Return 404 Not Found
return NotFound();
return NotFound(message);

// Return custom status code
return StatusCode(201);
return StatusCode(201, data);
```

### Dependency Injection

The framework includes a built-in DI container. Register dependencies during API initialization:

```csharp
_api = UserDefinedApi.CreateBuilder()
    .AddControllers()
    .AddRepository<IRepository, RepositoryImpl>()
    .ConfigureServices((services) =>
    {
        
    })
    .Build();
```

Access dependencies via constructor injection in your controllers:

```csharp
public class MyController : ControllerBase
{
    private readonly IRepository _repository;

    public MyController(IRepository repository)
    {
        _repository = repository;
    }
}
```

### OData Support

Query your data using OData conventions for filtering, sorting, and pagination.

### OpenAPI Documentation

The framework automatically generates OpenAPI documentation from your controllers, making it easy to understand and test your API.

## Project Structure

This repository contains three main projects:

- **SDM.UserDefinedApi** - Core interfaces and build-time components
- **SDM.UserDefinedApi.Runtime** - Runtime framework including controllers, routing, and DI
- **SDM.UserDefinedApi.Build** - MSBuild tasks for OpenAPI generation and packaging

## Advanced Features

### Custom Formatters

Implement custom input/output formatters by implementing `IInputConverter` and `IOutputConverter`:

```csharp
public class MyCustomFormatter : IOutputConverter
{
    public string Convert<T>(T value)
    {
        // Custom serialization logic
    }
}
```

### Access API Context

Access the underlying API request and response through the `ApiContext` property:

```csharp
public class MyController : ControllerBase
{
    public IActionResult MyAction()
    {
        var request = this.Request;  // ApiTriggerInput
        var response = this.Response;  // ApiTriggerOutput
        // ...
    }
}
```

## Requirements

- DataMiner 10.3.0 or higher
- .NET Framework 4.6.2 or higher / .NET Standard 2.0

## License

See [LICENSE.txt](SDM.UserDefinedApi.Runtime/LICENSE.txt) for license information.

## Support

For issues, questions, or contributions, please refer to the Skyline Communications support channels.
