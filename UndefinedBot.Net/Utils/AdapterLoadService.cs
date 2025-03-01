using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Net.Utils;

internal sealed class AdapterLoadService(ILogger<AdapterLoadService> logger, PluginLoadService pluginLoadService)
    : IDisposable
{
    private static string AdapterRoot => Path.Join(Environment.CurrentDirectory, "Adapters");

    private static string LibSuffix => GetLibSuffix();

    private readonly List<IAdapterInstance> _adapterInstances = [];
    private readonly PluginLoadService _pluginLoadService = pluginLoadService;

    internal void ExternalInvokeCommand(CommandInformation information, BaseCommandSource source)
    {
        IAdapterInstance? targetAdapter = _adapterInstances.Find(t => t.Id == information.AdapterId);
        if (targetAdapter is null)
        {
            logger.LogWarning("No such adapter: {AdapterId}",information.AdapterId);
            return;
        }

        targetAdapter.ExternalInvokeCommand(information, source);
    }

    public void LoadAdapter()
    {
        Unload();
        logger.LogInformation("Start loading adapters");
        if (!Directory.Exists(AdapterRoot))
        {
            Directory.CreateDirectory(AdapterRoot);
            logger.LogWarning("Adapters folder not fount, creating adapters folder.");
            return;
        }

        string[] adapterFolders = Directory.GetDirectories(AdapterRoot);
        foreach (string af in adapterFolders)
        {
            string adapterPropertiesFile = Path.Join(af, "adapter.json");
            logger.LogTrace("Adapter path: {af}", af);
            string sdkFile = Path.Join(af, $"UndefinedBot.Core.{LibSuffix}");
            FileIO.SafeDeleteFile(sdkFile);
            if (!File.Exists(adapterPropertiesFile))
            {
                logger.LogWarning("<{af}> not have adapter.json", af);
                continue;
            }

            JsonNode? originJson = FileIO.ReadAsJson(adapterPropertiesFile);
            string? ef = originJson?["EntryFile"]?.GetValue<string>();
            if (originJson is null || ef is null)
            {
                logger.LogWarning("<{af}> has invalid adapter.json", af);
                continue;
            }

            string entryFile = $"{Path.Join(af, ef)}.{LibSuffix}";
            if (!File.Exists(entryFile))
            {
                logger.LogWarning("Binary entry file: <{entryFile}> not found", entryFile);
                continue;
            }

            IAdapterInstance? inst = CreateAdapterInstance(entryFile);
            if (inst is null)
            {
                logger.LogWarning("<{af}> failed to create instance", af);
                continue;
            }

            //_adapterReferences[inst.Id] = adapterProperties;
            inst.MountCommands(new CommandManager(_pluginLoadService, inst));
            _adapterInstances.Add(inst);
            logger.LogInformation("Success Load Adapter: {Id}", inst.Id);
        }

        GC.Collect();
        string adapterListText = JsonSerializer.Serialize(_adapterInstances, UndefinedApp.SerializerOptions);
        FileIO.WriteFile(Path.Join(Environment.CurrentDirectory, "loaded_adapters.json"), adapterListText);
        logger.LogInformation("Loaded Adapters:{AdapterList}", adapterListText);
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
                logger.LogWarning("Entry point not found in assembly {adapterLibPath}", adapterLibPath);
                return null;
            }

            logger.LogTrace("Adapter class: {targetClass}", targetClass.FullName);
            //Create Adapter Instance
            if (Activator.CreateInstance(targetClass) is IAdapterInstance targetAdapterInstance)
            {
                logger.LogTrace("Adapter instance created: {targetAdapterInstance}", targetAdapterInstance.Id);
                return targetAdapterInstance;
            }

            logger.LogWarning("Fail to create instance from {targetClass}", targetClass.FullName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to Load Adapter From {adapterLibPath}", adapterLibPath);
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
    public void Unload()
    {
        foreach (var ai in _adapterInstances)
        {
            ai.Dispose();
        }
        _adapterInstances.Clear();
    }

    public void Dispose()
    {
        //NaN
    }
}