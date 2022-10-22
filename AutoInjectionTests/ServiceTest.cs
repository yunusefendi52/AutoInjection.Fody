using AutoInjection;

namespace AutoInjectionConsole;

public interface IMyService { }
public class MyService : IMyService { }

public interface IMyService2 { }
public class MyService2 : IMyService2 { }

public class MyClient
{
    public MyClient() { }

    public MyClient(
        string param,
        Dictionary<string, object> dict,
        Dictionary<string, object>? dict2 = null,
        [AutoInjection] IMyService? myService = null,
        [AutoInjection] IMyService? myService2 = null,
        [AutoInjection] Lazy<IMyService>? myServiceLazy = null,
        [AutoInjection] Lazy<IMyService>? myServiceLazy2 = null,
        [AutoInjection] Func<IMyService>? myServiceFunc = null,
        [AutoInjection] IMyService2? service2 = null)
    {
        ArgumentNullException.ThrowIfNull(myService);
        ArgumentNullException.ThrowIfNull(myService2);
        ArgumentNullException.ThrowIfNull(myServiceLazy);
        ArgumentNullException.ThrowIfNull(myServiceLazy.Value);
        ArgumentNullException.ThrowIfNull(myServiceLazy2);
        ArgumentNullException.ThrowIfNull(myServiceLazy2.Value);
        ArgumentNullException.ThrowIfNull(myServiceFunc);
        ArgumentNullException.ThrowIfNull(myServiceFunc.Invoke());
        ArgumentNullException.ThrowIfNull(service2);

        Param = param;
        Dict = dict;
        Dict2 = dict2;
    }

    public string Param { get; }

    public Dictionary<string, object> Dict { get; }
    public Dictionary<string, object>? Dict2 { get; }
}
