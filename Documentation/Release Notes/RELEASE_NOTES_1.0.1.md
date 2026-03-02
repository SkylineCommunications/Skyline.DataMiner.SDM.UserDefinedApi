# Release Notes - Version 1.0.1

Release Date: March 2, 2026

## 🎉 First Official Release

This marks the first official stable release of **Skyline.DataMiner.SDM.UserDefinedApi**, a comprehensive framework for building User-Defined APIs in DataMiner with a controller-based approach similar to ASP.NET Core.

## ✨ Key Features

- **Controller-based architecture**: Define API endpoints using familiar controller classes with attribute routing
- **Dependency injection**: Built-in DI container support for services and repositories
- **Automatic OpenAPI documentation**: Generate OpenAPI specifications from your controllers
- **OData support**: Query your data using OData conventions (limited)

## 🚀 Getting Started

Install the NuGet package:

```bash
dotnet add package Skyline.DataMiner.SDM.UserDefinedApi
```

### Enabling OpenAPI Generation

To enable automatic OpenAPI specification generation during build, add the following to your `.csproj` file:

```xml
<PropertyGroup>
  <GenerateOpenApi>true</GenerateOpenApi>
</PropertyGroup>
```

This will generate an `openapi.yaml` file in your build output directory.