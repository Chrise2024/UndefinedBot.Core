﻿using System.Reflection;
using System.Text.Json.Nodes;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Net.Utils;

internal static class AdapterLoader
{
    private static string AdapterRoot => Path.Join(Environment.CurrentDirectory, "Adapters");
    private static string LibSuffix => GetLibSuffix();
    private static ILogger AdapterInitializeLogger => new ExtendableLogger(["Init","Load Adapter"]);
    public static List<IAdapterInstance> LoadAdapters()
    {
        List<IAdapterInstance> adapterInstances = [];
        AdapterInitializeLogger.Info("Start Loading Adapters");
        if (!Directory.Exists(AdapterRoot))
        {
            Directory.CreateDirectory(AdapterRoot);
            AdapterInitializeLogger.Warn("Adapters Folder Not Fount, Creating Adapters Folder.");
            return [];
        }

        string[] adapterFolders = Directory.GetDirectories(AdapterRoot);
        foreach (string af in adapterFolders)
        {
            string adapterPropertiesFile = Path.Join(af, "adapter.json");
            string sdkFile = Path.Join(af, $"UndefinedBot.Core.{LibSuffix}");
            FileIO.SafeDeleteFile(sdkFile);
            if (!File.Exists(adapterPropertiesFile))
            {
                AdapterInitializeLogger.Warn($"Adapter: <{af}> Not Have adapter.json");
                continue;
            }

            JsonNode? originJson = FileIO.ReadAsJson(adapterPropertiesFile);
            string? ef = originJson?["EntryFile"]?.GetValue<string>();
            if (originJson is null || ef is null)
            {
                AdapterInitializeLogger.Warn($"Adapter: <{af}> Invalid adapter.json");
                continue;
            }
            
            string entryFile = $"{Path.Join(af, ef)}.{LibSuffix}";
            if (!File.Exists(entryFile))
            {
                AdapterInitializeLogger.Warn($"Adapter: <{af}> Binary EntryFile: <{entryFile}> Not Found");
                continue;
            }

            IAdapterInstance? inst = CreateAdapterInstance(entryFile);
            if (inst is null)
            {
                AdapterInitializeLogger.Warn($"Adapter: <{af}> Load Failed");
                continue;
            }

            //_adapterReferences[inst.Id] = adapterProperties;
            adapterInstances.Add(inst);
            AdapterInitializeLogger.Info($"Success Load Adapter: {inst.Id}");
        }

        //AssemblyLoadContext.Unload();
        GC.Collect();
        //Mount AdapterInstances on ActionManager to Handle Plugin's Action
        ActionManager.UpdateAdapterInstances(adapterInstances);
        return adapterInstances;
    }

    private static IAdapterInstance? CreateAdapterInstance(string adapterLibPath)
    {
        try
        {
            //Get Adapter Class
            Type targetClass = Assembly
                                   .LoadFrom(adapterLibPath)
                                   .GetTypes()
                                   .ToList()
                                   .Find(type => type.BaseType?.FullName == "UndefinedBot.Core.Adapter.BaseAdapter") ??
                               throw new TypeLoadException(adapterLibPath);
            //Create Adapter Instance
            IAdapterInstance targetAdapterInstance =
                Activator.CreateInstance(targetClass) as IAdapterInstance ??
                throw new TypeInitializationException(targetClass.FullName, null);
            return targetAdapterInstance;
        }
        catch (TargetInvocationException tie)
        {
            AdapterInitializeLogger.Error(tie.InnerException, "Adapter's Constructor Occurs Exception");
        }
        catch (TypeLoadException tle)
        {
            AdapterInitializeLogger.Error(tle, "Unable to Find Specific Adapter Class");
        }
        catch (TypeInitializationException tie)
        {
            AdapterInitializeLogger.Error(tie, "Unable to Create Adapter Instance");
        }
        catch (MethodAccessException)
        {
            AdapterInitializeLogger.Error("Unable to Find Specific Adapter Handler Method");
        }
        catch (Exception ex)
        {
            AdapterInitializeLogger.Error(ex, $"Unable to Load Adapter From {adapterLibPath}");
        }

        return null;
    }

    private static string GetLibSuffix() => "dll";
    // .Net dll on Unix is still .dll extension(?
    /*{
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "dll";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "so";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "so";
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
    }*/
}