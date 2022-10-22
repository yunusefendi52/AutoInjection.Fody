namespace AutoInjection;


public class AutoInjectionAttribute : Attribute
{
    public AutoInjectionAttribute() { }

    // public AutoInjectionAttribute(string name) { }
}

public class AutoInjection
{
    static AutoInjection? instance;
    public static AutoInjection Instance
    {
        get
        {
            if (instance == null)
                instance = new();
            return instance;
        }
    }

    readonly List<IServiceProvider> providers = new();

    public object? GetService(Type type)
    {
        foreach (var provider in providers)
        {
            var service = provider.GetService(type);
            if (service != null)
                return service;
        }

        return null;
    }

    public T? GetService<T>()
    {
        var service = GetService(typeof(T));
        if (service != null)
            return (T)service;
        return default;
    }

    public Lazy<T?> GetServiceAsLazy<T>()
    {
        var l = new Lazy<T?>(() =>
        {
            var service = GetService<T>();
            return service;
        });
        return l;
    }

    public Func<T?> GetServiceAsFunc<T>()
    {
        var l = new Func<T?>(() =>
        {
            var service = GetService<T>();
            return service;
        });
        return l;
    }

    public void Register(IServiceProvider service)
    {
        providers.Add(service);
    }

    public void Unregister(IServiceProvider service)
    {
        providers.Remove(service);
    }
}