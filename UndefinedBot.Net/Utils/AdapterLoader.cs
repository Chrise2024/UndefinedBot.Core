using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Net.Utils;

internal sealed class AdapterLoader(ILogger<AdapterLoader> logger) : IDisposable
{
    private static string AdapterRoot => Path.Join(Environment.CurrentDirectory, "Adapters");

    private static string LibSuffix => GetLibSuffix();

    //private static InternalLogger AdapterInitializeLogger => new(["Init","Load Adapter"]);
    private ILogger<AdapterLoader> Logger => logger;

    public List<IAdapterInstance> LoadAdapters()
    {

        List<IAdapterInstance> adapterInstances = [];
        Logger.LogInformation("Start loading adapters");
        if (!Directory.Exists(AdapterRoot))
        {
            Directory.CreateDirectory(AdapterRoot);
            Logger.LogWarning("Adapters folder not fount, creating adapters folder.");
            return [];
        }

        string[] adapterFolders = Directory.GetDirectories(AdapterRoot);
        foreach (string af in adapterFolders)
        {
            string adapterPropertiesFile = Path.Join(af, "adapter.json");
            Logger.LogTrace("Adapter path: {af}", af);
            string sdkFile = Path.Join(af, $"UndefinedBot.Core.{LibSuffix}");
            FileIO.SafeDeleteFile(sdkFile);
            if (!File.Exists(adapterPropertiesFile))
            {
                Logger.LogWarning("<{af}> not have adapter.json", af);
                continue;
            }

            JsonNode? originJson = FileIO.ReadAsJson(adapterPropertiesFile);
            string? ef = originJson?["EntryFile"]?.GetValue<string>();
            if (originJson is null || ef is null)
            {
                Logger.LogWarning("<{af}> has invalid adapter.json", af);
                continue;
            }

            string entryFile = $"{Path.Join(af, ef)}.{LibSuffix}";
            if (!File.Exists(entryFile))
            {
                Logger.LogWarning("Binary entry file: <{entryFile}> not found", entryFile);
                continue;
            }

            IAdapterInstance? inst = CreateAdapterInstance(entryFile);
            if (inst is null)
            {
                Logger.LogWarning("<{af}> failed to create instance", af);
                continue;
            }

            //_adapterReferences[inst.Id] = adapterProperties;
            adapterInstances.Add(inst);
            Logger.LogInformation("Success Load Adapter: {Id}", inst.Id);
        }

        //AssemblyLoadContext.Unload();
        GC.Collect();
        //Mount AdapterInstances on ActionManager to Handle Plugin's Action
        ActionManager.UpdateAdapterInstances(adapterInstances);
        return adapterInstances;
    }

    private IAdapterInstance? CreateAdapterInstance(string adapterLibPath)
    {
        try
        {
            //Get Adapter Class
            Type? targetClass = Assembly
                .LoadFrom(adapterLibPath)
                .GetTypes()
                .ToList()
                .Find(type => type.BaseType?.FullName == "UndefinedBot.Core.Adapter.BaseAdapter");
            if (targetClass is null)
            {
                Logger.LogWarning("Entry point not found in assembly {adapterLibPath}", adapterLibPath);
                return null;
            }

            Logger.LogTrace("Adapter class: {targetClass}", targetClass.FullName);
            //Create Adapter Instance
            if (Activator.CreateInstance(targetClass) is IAdapterInstance targetAdapterInstance)
            {
                Logger.LogTrace("Adapter instance created: {targetAdapterInstance}", targetAdapterInstance.Id);
                return targetAdapterInstance;
            }

            Logger.LogWarning("Fail to create instance from {targetClass}", targetClass.FullName);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Unable to Load Adapter From {adapterLibPath}", adapterLibPath);
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
    public void Dispose()
    {
        ActionManager.DisposeAdapterInstance();
        GC.SuppressFinalize(this);
    }
}