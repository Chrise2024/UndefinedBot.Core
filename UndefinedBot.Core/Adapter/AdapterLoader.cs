using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Adapter;

internal static class AdapterManager
{
    private static string AdapterRoot => Path.Join(Environment.CurrentDirectory, "Adapters");
    private static string LibSuffix => GetLibSuffix();
    private static GeneralLogger AdapterLoaderLogger => new("AdapterLoad");
    private static Dictionary<string, AdapterInstance> AdapterInstances { get; set; } = [];
    private static AdapterConfigData DefaultAdapterMeta => new();

    public static string? HandlerDefaultAction(string adapterId, DefaultActionType action, object paras)
    {
        try
        {
            return AdapterInstances.TryGetValue(adapterId, out AdapterInstance inst) ? JsonSerializer.Serialize(inst.DefaultActionHandler.Invoke(inst.Instance, [action, paras])) : null;
        }
        catch (Exception)
        {
            AdapterLoaderLogger.Error("Fail to Invoke Action");
        }

        return null;
    }
    public static string? HandlerCustomAction(string adapterId, string action, object paras)
    {
        try
        {
            return AdapterInstances.TryGetValue(adapterId, out AdapterInstance inst)
                ? JsonSerializer.Serialize(inst.CustomActionHandler.Invoke(inst.Instance, [action, paras]))
                : null;
        }
        catch (Exception)
        {
            AdapterLoaderLogger.Error("Fail to Invoke Action");
        }

        return null;
    }

    public static void LoadAdapters()
    {
        AdapterLoaderLogger.Info("Start Loading Adapters");
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
                AdapterLoaderLogger.Warn($"Adapter: <{af}> Not Have adapter.json");
                continue;
            }
            AdapterConfigData? adapterMetaProperties = FileIO.ReadAsJson<AdapterConfigData>(adapterPropertiesFile);
            if (adapterMetaProperties == null || adapterMetaProperties.Equals(DefaultAdapterMeta))
            {
                AdapterLoaderLogger.Warn($"Adapter: <{af}> Invalid adapter.json");
                continue;
            }
            string entryFile = $"{Path.Join(af, adapterMetaProperties.EntryFile)}.{LibSuffix}";
            if (!File.Exists(entryFile))
            {
                AdapterLoaderLogger.Warn($"Adapter: <{af}> Binary EntryFile: <{entryFile}> Not Found");
                continue;
            }

            (string id,AdapterInstance? inst) = CreateAdapterInstance(entryFile, adapterMetaProperties);
            if (inst == null)
            {
                AdapterLoaderLogger.Warn($"Adapter: <{af}> Load Failed");
                continue;
            }
            AdapterInstances[id] = (AdapterInstance)inst;
            AdapterLoaderLogger.Info($"Success Load Adapter: {id}");
        }

    }

    private static (string,AdapterInstance?) CreateAdapterInstance(string adapterLibPath,AdapterConfigData adapterConfig)
    {
        try
        {
            Type targetClass = Assembly
                .LoadFrom(adapterLibPath)
                .GetTypes()
                .ToList()
                .Find(type => type.BaseType?.FullName == "UndefinedBot.Core.Adapter.BaseAdapter") ?? throw new TypeLoadException(adapterLibPath);
            object targetClassInstance =
                Activator.CreateInstance(targetClass,[adapterConfig]) ??
                throw new TypeInitializationException(targetClass.FullName, null);
            MethodInfo defaultHandler = targetClass.GetMethod("HandleDefaultAction") ??
                                    throw new MethodAccessException();
            MethodInfo customHandler = targetClass.GetMethod("HandleCustomAction") ??
                                       throw new MethodAccessException();
            string aid = targetClass.GetProperty("Id")?.GetValue(targetClassInstance) as string ??
                         throw new MethodAccessException();
            return (aid,new AdapterInstance(targetClassInstance, defaultHandler, customHandler));
        }
        catch (TypeLoadException tle)
        {
            AdapterLoaderLogger.Error($"Unable to Find Specific Adapter Class: {tle.Message}");
        }
        catch (TypeInitializationException tie)
        {
            AdapterLoaderLogger.Error($"Unable to Create Adapter Instance: {tie.Message}");
        }
        catch (MethodAccessException)
        {
            AdapterLoaderLogger.Error("Unable to Find Specific Adapter Handler Method");
        }
        catch (Exception)
        {
            AdapterLoaderLogger.Error($"Unable to Load Command From {adapterLibPath}");
        }

        return ("",null);
    }
    private static string GetLibSuffix()
    {
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
    }
}

internal readonly struct AdapterInstance(object instance,MethodInfo dah,MethodInfo cah)
{
    internal object Instance => instance;
    internal MethodInfo DefaultActionHandler => dah;
    internal MethodInfo CustomActionHandler => cah;
}
public enum DefaultActionType
{
    SendPrivateMsg = 0,
    SendGroupMsg = 1,
    RecallMessage = 2,
    GetMessage = 3,
    GetGroupMemberInfo = 4,
    GetGroupMemberList = 5,
    GroupMute = 6,
    GroupKick = 7,
}
