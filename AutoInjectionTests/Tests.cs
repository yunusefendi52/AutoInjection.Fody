using System.Diagnostics;
using AutoInjection;
using AutoInjectionConsole;
using DryIoc;
using Splat;

namespace AutoInjectionTests;

public class Tests
{
    static void Setup()
    {
        Pure.DI.DI.Setup("TestsDI")
            .Bind<IMyService2>().To<MyService2>();
    }

    public Tests()
    {
        var dryioc = new DryIoc.Container();
        dryioc.Register<IMyService, MyService>();
        var provider = dryioc as IServiceProvider;
        AutoInjection.AutoInjection.Instance.Register(provider);

        var pureDIServiceProvider = TestsDI.ResolveIServiceProvider();
        AutoInjection.AutoInjection.Instance.Register(pureDIServiceProvider);
    }

    [Fact]
    public void CanResolve()
    {
        string param = "Hello service provider";
        var dict = new Dictionary<string, object>()
        {
            { "key1", "value1" },
        };
        var dict2 = new Dictionary<string, object>()
        {
            { "key", "value" },
        };
        var m = new MyClient(
            param,
            dict,
            dict2
        );
        Assert.NotNull(m);
        Assert.Equal(m.Param, param);
        Assert.Same(m.Dict, dict);
        Assert.Same(m.Dict2, dict2);
    }

    [Fact]
    public void AutoInjectionAttributeDeleted()
    {
        var m = new MyClient();
        foreach (var ctor in m.GetType().Constructors())
        {
            foreach (var item in ctor.GetParameters())
            {
                if (item.Name != null && item.Name.Contains("Service", StringComparison.InvariantCultureIgnoreCase) && item.CustomAttributes.Any(v => v.AttributeType.Name.Contains("AutoInjection")))
                {
                    Assert.True(false, $"Still has AutoInjection attribute {item.Name}");
                }
            }
        }
    }
}
