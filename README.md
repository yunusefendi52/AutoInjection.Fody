# AutoInjection

![https://github.com/yunusefendi52/AutoInjection.Fody/actions/workflows/main.yml/badge.svg](https://github.com/yunusefendi52/AutoInjection.Fody/actions/workflows/main.yml/badge.svg)

[![Nuget](https://img.shields.io/nuget/v/AutoInjection.Fody?style=for-the-badge)](https://www.nuget.org/packages/AutoInjection.Fody)

### Installation
```xml
<PackageReference Include="AutoInjection.Fody" Version="1.0.0-alpha2" />
```

Auto inject service to class with `IServiceProvider`

Register your service provider
```csharp
AutoInjection.Register(serviceProvider);
```

Decorate parameter constructor
```csharp
public class MyViewModel
{
    public MyViewModel([AutoInjection] IApiService apiService = null)
    {
    }
}
```

At compile time it will inject the service provider to get the service, like
```csharp
public class MyViewModel
{
    public MyViewModel([AutoInjection] IApiService apiService = null)
    {
        if (apiService == null)
        {
            apiService = AutoInjection.GetService<IApiService>();
        }
    }
}
```
