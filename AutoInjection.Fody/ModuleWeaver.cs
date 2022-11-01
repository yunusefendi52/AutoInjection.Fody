using Fody;

namespace AutoInjection.Fody;

public class ModuleWeaver : BaseModuleWeaver
{
    public override void Execute()
    {
        Injector.Inject(
            ModuleDefinition,
            (v) =>
            {
                WriteMessage(v, MessageImportance.Low);
            },
            (v) =>
            {
                WriteError(v);
            });
    }

    public override bool ShouldCleanReference => false;

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "mscorlib";
        yield return "netstandard";
        yield return "System";
        yield return "System.Runtime";
        yield return "AutoInjection";
        yield return "AutoInjection.Helpers";
    }
}
