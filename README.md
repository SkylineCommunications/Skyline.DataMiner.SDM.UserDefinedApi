# Skyline.DataMiner.SDM.UserDefinedApi

> [!WARNING]
> **Deprecated:** This repository and NuGet package are no longer maintained and have been archived. The functionality has been split into two separate packages:
>
> - [Skyline.DataMiner.SDM.OData](https://github.com/SkylineCommunications/Skyline.DataMiner.SDM.OData) — OData query support.
> - [Skyline.DataMiner.Utils.UserDefinedApiToolkit](https://github.com/SkylineCommunications/Skyline.DataMiner.Utils.UserDefinedApiToolkit) — the controller-based User-Defined API framework (routing, DI, OpenAPI generation).
>
> Please migrate to these packages for new development and future updates.

A framework for building User-Defined APIs in DataMiner, providing a controller-based approach similar to ASP.NET Core for creating RESTful APIs.

## Migration Guide

The controller/routing/DI framework and the OData query support that used to live together in this package have been split into two independently maintained packages:

| Old (this package) | New package | Covers |
| --- | --- | --- |
| `Skyline.DataMiner.SDM.UserDefinedApi` (controllers, routing, DI, OpenAPI) | [`Skyline.DataMiner.Utils.UserDefinedApiToolkit`](https://github.com/SkylineCommunications/Skyline.DataMiner.Utils.UserDefinedApiToolkit) | Attribute routing, DI, typed results, OpenAPI generation |
| `Skyline.DataMiner.SDM.UserDefinedApi` (OData support) | [`Skyline.DataMiner.SDM.OData`](https://github.com/SkylineCommunications/Skyline.DataMiner.SDM.OData) | `$filter`/`$orderby` parsing and translation to `SLDataGateway` queries |

### Steps

1. **Update package references**
   ```bash
   dotnet remove package Skyline.DataMiner.SDM.UserDefinedApi
   dotnet add package Skyline.DataMiner.Utils.UserDefinedApiToolkit
   # Only if you were using OData query support:
   dotnet add package Skyline.DataMiner.SDM.OData
   ```

2. **Update namespaces**
   Replace `using Skyline.DataMiner.SDM.UserDefinedApi;` (and `...DI;`) with:
   ```csharp
   using Skyline.DataMiner.Utils.UserDefinedApiToolkit;
   ```
   The entry point (`UserDefinedApi.CreateBuilder().AddControllers().Build()` called from `OnApiTrigger`), controller base class, attributes (`[ApiController]`, `[Route]`, `[HttpGet]`, etc.) and DI (`ConfigureServices`) keep the same shape, just under the new namespace/package.

3. **Update action return types**
   Controllers in the new toolkit return typed `IApiResult` / `ApiResult<TSuccess>` / `ApiResult<TSuccess, TError>` instead of `IActionResult`:
   ```csharp
   [HttpGet]
   public ApiResult<UserDto, string> GetById([FromQuery] int id)
   {
       var user = _repository.GetById(id);
       return user is null ? NotFound("User not found.") : Ok(user);
   }
   ```

4. **Move OData filtering/sorting logic**
   If you used the built-in OData query support, switch to `ODataSdmTranslator<T>` from `Skyline.DataMiner.SDM.OData` to translate `$filter`/`$orderby` strings into `IQuery<T>` instances for `SLDataGateway`.

5. **Services registration**
   Repositories registered via `ConfigureServices` continue to work the same way; check the [`Skyline.DataMiner.Utils.UserDefinedApiToolkit` README](https://github.com/SkylineCommunications/Skyline.DataMiner.Utils.UserDefinedApiToolkit) for the current recommended registration patterns.

For full details and additional examples, see the READMEs of the two new repositories linked above.


> [!NOTE]
> The remainder of this README describes the original, now deprecated, `Skyline.DataMiner.SDM.UserDefinedApi` package and is kept for historical/archival reference only.

---
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

    [HttpPut]
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

    [HttpDelete]
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
- Route parameters - Currently not supported in User-Defined APIs

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
        services.AddSingleton<MySingletonClass>();
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

The framework can automatically generate OpenAPI 3.0 documentation from your controllers during the build process.

#### Enabling OpenAPI Generation

To enable OpenAPI generation, add the following property to your `.csproj` file:

```xml
<PropertyGroup>
  <GenerateOpenApi>True</GenerateOpenApi>
</PropertyGroup>
```

By default, this will generate an `openapi.yaml` file in your build output directory under the `openapi` folder.

#### Specifying the Output Format

You can specify the output format (YAML or JSON) using the `OpenApiFormat` property:

```xml
<PropertyGroup>
  <GenerateOpenApi>True</GenerateOpenApi>
  <OpenApiFormat>json</OpenApiFormat>  <!-- Options: yaml (default) or json -->
</PropertyGroup>
```

The generated OpenAPI specification will include:
- All API endpoints from your controllers
- HTTP methods and route patterns
- Request and response schemas
- Parameter definitions
- Model descriptions

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

## About DataMiner

DataMiner is a transformational platform that provides vendor-independent control and monitoring of devices and services. Out of the box and by design, it addresses key challenges such as security, complexity, multi-cloud, and much more. It has a pronounced open architecture and powerful capabilities enabling users to evolve easily and continuously.

The foundation of DataMiner is its powerful and versatile data acquisition and control layer. With DataMiner, there are no restrictions to what data users can access. Data sources may reside on premises, in the cloud, or in a hybrid setup.

A unique catalog of 7000+ connectors already exists. In addition, you can leverage DataMiner Development Packages to build your own connectors (also known as "protocols" or "drivers").

> **Note**
> See also: [About DataMiner](https://aka.dataminer.services/about-dataminer).

## About Skyline Communications

At Skyline Communications, we deal in world-class solutions that are deployed by leading companies around the globe. Check out [our proven track record](https://aka.dataminer.services/about-skyline) and see how we make our customers' lives easier by empowering them to take their operations to the next level.