using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Cecilifier.Runtime;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace AutoInjection;

public static class Injector
{
    public static bool Inject(
        ModuleDefinition mainModule,
        Action<string>? logInfo = null,
        Action<string>? logError = null)
    {
        try
        {
            var changed = false;

            var helpers = mainModule.AssemblyReferences.Where(x => x.Name == "AutoInjection").FirstOrDefault();
            if (helpers is null)
            {
                logInfo?.Invoke("Could not find assembly: AutoInjection (" + string.Join(", ", mainModule.AssemblyReferences.Select(x => x.Name)) + ")");
                return false;
            }

            foreach (var type in mainModule.Types)
            {
                var ctor = type.Methods.FirstOrDefault();
                if (ctor is null || !ctor.Parameters.Any(param => param.CustomAttributes.Any(v => v.AttributeType.Name.Contains("AutoInjection"))))
                {
                    logInfo?.Invoke($"No ctor with attribute found, skipping {type.FullName}");
                    continue;
                }

                changed = true;
                logInfo?.Invoke($"Ctor found {type.FullName}");

                var il_ctor_2 = ctor.Body.GetILProcessor();
                // var lastInstruction = il_ctor_2.Body.Instructions.Last();
                var prevInstructions = ctor.Body.Instructions.ToList();
                ctor.Body.Instructions.Clear();

                logInfo?.Invoke("Adding injection");
                foreach (var param in ctor.Parameters)
                {
                    if (param.CustomAttributes.Any(v => v.AttributeType.Name.Contains("AutoInjection")))
                    {
                        //if (myService == null)
                        il_ctor_2.Emit(OpCodes.Ldarg, param);
                        il_ctor_2.Emit(OpCodes.Ldnull);
                        il_ctor_2.Emit(OpCodes.Ceq);
                        var lbl_elseEntryPoint_4 = il_ctor_2.Create(OpCodes.Nop);
                        il_ctor_2.Emit(OpCodes.Brfalse, lbl_elseEntryPoint_4);
                        //if body

                        var helpersAssembly = mainModule.AssemblyResolver.Resolve(helpers);
                        var helpersAssemblyTypes = helpersAssembly.MainModule.Types;
                        var autoInjectionMethods = helpersAssemblyTypes.Where(v =>
                        {
                            return v.FullName == "AutoInjection.AutoInjection";
                        }).Select(v => v).SelectMany(v =>
                        {
                            return v.Methods;
                        });
                        var instanceField = autoInjectionMethods.Where(v => v.Name == "get_Instance").First();
                        // var instanceFieldFromType = TypeHelpers.ResolveMethod(
                        //     $"AutoInjection.AutoInjection, {helpers.Name}",
                        //     "get_Instance",
                        //     System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
                        //     "");
                        var autoInjectionField = mainModule.ImportReference(instanceField);
                        il_ctor_2.Emit(OpCodes.Call, autoInjectionField);
                        var isLazy = param.ParameterType.FullName.StartsWith("System.Lazy");
                        if (param.ParameterType.IsGenericInstance
                            && (isLazy || param.ParameterType.FullName.StartsWith("System.Func"))
                            && param.ParameterType is GenericInstanceType g)
                        {
                            var paramRealType = g.GenericArguments.First();
                            var getServiceMethodRef = autoInjectionMethods.Where(v =>
                            {
                                return v.HasGenericParameters && (isLazy ? v.Name == "GetServiceAsLazy" : v.Name == "GetServiceAsFunc");
                            }).First();
                            var getServiceGenericMethod = getServiceMethodRef.MakeGenericMethod(new[]{
                                paramRealType,
                            });
                            var getServiceMethod = mainModule.ImportReference(getServiceGenericMethod);
                            il_ctor_2.Emit(OpCodes.Call, getServiceMethod);
                            il_ctor_2.Emit(OpCodes.Starg_S, param);
                        }
                        else
                        {
                            var getServiceMethodRef = autoInjectionMethods.Where(v =>
                            {
                                return v.HasGenericParameters && v.Name == "GetService";
                            }).First();
                            var getServiceGenericMethod = getServiceMethodRef.MakeGenericMethod(new[]{
                                param.ParameterType,
                            });
                            var getServiceMethod = mainModule.ImportReference(getServiceGenericMethod);
                            il_ctor_2.Emit(OpCodes.Call, getServiceMethod);
                            il_ctor_2.Emit(OpCodes.Starg_S, param);
                        }
                        var lbl_elseEnd_6 = il_ctor_2.Create(OpCodes.Nop);
                        il_ctor_2.Append(lbl_elseEntryPoint_4);
                        il_ctor_2.Append(lbl_elseEnd_6);
                    }
                }

                logInfo?.Invoke("Done adding injection");

                logInfo?.Invoke("Rewrite previous instructions");
                foreach (var item in prevInstructions)
                {
                    il_ctor_2.Append(item);
                }

                logInfo?.Invoke("OptimizeMacros");
                ctor.Body.OptimizeMacros();
            }

            return changed;
        }
        catch (Exception ex)
        {
            logError?.Invoke(ex.ToString());
            throw;
        }
    }
}