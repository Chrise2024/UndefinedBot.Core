using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Net.Utils;

internal static class AdapterLoader
{
    private static string AdapterRoot => Path.Join(Environment.CurrentDirectory, "Adapters");
    private static string LibSuffix => GetLibSuffix();
    private static GeneralLogger AdapterInitializeLogger => new("AdapterLoad");
    private static readonly Dictionary<string, AdapterInstance> s_adapterInstances = [];
    private static readonly Dictionary<string, AdapterProperties> s_adapterReferences = [];

    public static Dictionary<string, AdapterProperties> LoadAdapters()
    {
        AdapterInitializeLogger.Info("Start Loading Adapters");
        if (!Directory.Exists(AdapterRoot))
        {
            Directory.CreateDirectory(AdapterRoot);
            throw new FileNotFoundException("Adapters Folder Not Fount, Restart.");
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
            AdapterConfigData? adapterConfigData = originJson?.Deserialize<AdapterConfigData>();
            if (originJson == null || adapterConfigData == null || !adapterConfigData.IsValid())
            {
                AdapterInitializeLogger.Warn($"Adapter: <{af}> Invalid adapter.json");
                continue;
            }

            adapterConfigData.OriginalConfig = originJson;
            string entryFile = $"{Path.Join(af, adapterConfigData.EntryFile)}.{LibSuffix}";
            if (!File.Exists(entryFile))
            {
                AdapterInitializeLogger.Warn($"Adapter: <{af}> Binary EntryFile: <{entryFile}> Not Found");
                continue;
            }
            (AdapterProperties? adapterProperties,AdapterInstance? inst) = CreateAdapterInstance(entryFile, adapterConfigData);
            if (adapterProperties == null || inst == null)
            {
                AdapterInitializeLogger.Warn($"Adapter: <{af}> Load Failed");
                continue;
            }

            s_adapterReferences[adapterProperties.Id] = adapterProperties;
            s_adapterInstances[adapterProperties.Id] = inst;
            AdapterInitializeLogger.Info($"Success Load Adapter: {adapterProperties.Id}");
        }
        //Mount AdapterInstances on ActionManager to Handle Plugin's Action
        ActionManager.UpdateAdapterInstances(s_adapterInstances);
        return s_adapterReferences;
    }

    private static (AdapterProperties?,AdapterInstance?) CreateAdapterInstance(string adapterLibPath,AdapterConfigData config)
    {
        try
        {
            //Get Adapter Class
            Type targetClass = Assembly
                .LoadFrom(adapterLibPath)
                .GetTypes()
                .ToList()
                .Find(type => type.BaseType?.FullName == "UndefinedBot.Core.Adapter.BaseAdapter") ?? throw new TypeLoadException(adapterLibPath);
            //Create Adapter Instance
            object targetClassInstance =
                Activator.CreateInstance(targetClass,[config]) ??
                throw new TypeInitializationException(targetClass.FullName, null);
            //Get Action Handler
            MethodInfo defaultHandler = targetClass.GetMethod("HandleDefaultAction") ??
                                    throw new MethodAccessException();
            MethodInfo customHandler = targetClass.GetMethod("HandleCustomAction") ??
                                       throw new MethodAccessException();
            return (new AdapterProperties((targetClassInstance as BaseAdapter)!,config),new AdapterInstance(targetClassInstance, defaultHandler, customHandler));
        }
        catch (TypeLoadException tle)
        {
            AdapterInitializeLogger.Error($"Unable to Find Specific Adapter Class: {tle.Message}");
        }
        catch (TypeInitializationException tie)
        {
            AdapterInitializeLogger.Error($"Unable to Create Adapter Instance: {tie.Message}");
        }
        catch (MethodAccessException)
        {
            AdapterInitializeLogger.Error("Unable to Find Specific Adapter Handler Method");
        }
        catch (Exception ex)
        {
            AdapterInitializeLogger.Error($"Unable to Load Adapter From {adapterLibPath}");
            Console.WriteLine(ex.Message);
        }
        return (null,null);
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
