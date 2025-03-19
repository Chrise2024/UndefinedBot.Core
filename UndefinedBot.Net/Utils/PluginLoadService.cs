using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.MessageProcessor;
using UndefinedBot.Core.Plugin;
using InternalILoggerFactory = UndefinedBot.Core.Utils.ILoggerFactory;

namespace UndefinedBot.Net.Utils;

internal sealed class PluginLoadService(IServiceProvider provider) : IDisposable
{
    private static readonly string _pluginRoot = Path.Join(Environment.CurrentDirectory, "Plugins");
    private static readonly string _programCache = Path.Join(Environment.CurrentDirectory, "Cache");
    private static string LibSuffix => GetLibSuffix();

    private readonly List<IPluginInstance> _pluginInstanceList = [];
    private readonly List<CommandInstance> _commandInstanceList = [];
    private readonly List<MessageProcessorInstance> _processorInstanceList = [];
    private readonly Dictionary<string, List<CommandInstance>> _commandIndex = [];
    private readonly Dictionary<string, List<MessageProcessorInstance>> _processorIndex = [];
    private readonly ILogger<PluginLoadService> _logger = provider.GetRequiredService<ILogger<PluginLoadService>>();

    public List<CommandInstance> AcquireCommandInstance(string adapterId)
    {
        return _commandIndex.TryGetValue(adapterId, out List<CommandInstance>? v) ? v : [];
    }

    public List<MessageProcessorInstance> AcquireMessageProcessorInstance(string adapterId)
    {
        return _processorIndex.TryGetValue(adapterId, out List<MessageProcessorInstance>? v) ? v : [];
    }

    public void LoadPlugin()
    {
        Unload();
        _logger.LogInformation("Start loading plugins");
        if (!Directory.Exists(_pluginRoot))
        {
            Directory.CreateDirectory(_pluginRoot);
            _logger.LogWarning("Plugins folder not fount, creating Plugins folder.");
            return;
        }

        string[] pluginFolders = Directory.GetDirectories(_pluginRoot);
        foreach (string pf in pluginFolders)
        {
            string pluginPropertiesFile = Path.Join(pf, "plugin.json");
            //Remove SDK Lib to Let Plugin Call Main Program's UndefinedBot.Core Lib
            string pluginSdkFile = Path.Join(pf, $"UndefinedBot.Core.{LibSuffix}");
            FileIO.SafeDeleteFile(pluginSdkFile);

            if (!File.Exists(pluginPropertiesFile))
            {
                _logger.LogWarning("<{pf}> not have plugin.json", pf);
                continue;
            }

            JsonNode? originJson = FileIO.ReadAsJson(pluginPropertiesFile);
            string? ef = originJson?["EntryFile"]?.GetValue<string>();
            if (originJson is null || ef is null)
            {
                _logger.LogWarning("<{pf}> has invalid plugin.json", pf);
                continue;
            }

            string entryFile = $"{Path.Join(pf, ef)}.{LibSuffix}";
            if (!File.Exists(entryFile))
            {
                _logger.LogWarning("Binary entryFile: <{entryFile}> not found", entryFile);
                continue;
            }

            IPluginInstance? pluginInstance = CreatePluginInstance(entryFile);
            if (pluginInstance is null)
            {
                _logger.LogWarning("<{pf}> failed to create instance", pf);
                continue;
            }

            pluginInstance.Initialize();
            _commandInstanceList.AddRange(pluginInstance.GetCommandInstance());
            _processorInstanceList.AddRange(pluginInstance.GetMessageProcessorInstance());
            _pluginInstanceList.Add(pluginInstance);

            string pluginCachePath = Path.Join(_programCache, pluginInstance.Id);
            FileIO.EnsurePath(pluginCachePath);
            foreach (string cf in Directory.GetFiles(pluginCachePath)) FileIO.SafeDeleteFile(cf);
        }

        string pluginListText = JsonSerializer.Serialize(_pluginInstanceList, UndefinedApp.SerializerOptions);
        string commandListText = JsonSerializer.Serialize(
            _commandInstanceList.Select(c => $"{c.PluginId}/{c.Name} - {JsonSerializer.Serialize(c.TargetAdapterId)}"),
            UndefinedApp.SerializerOptions);
        string processorListText = JsonSerializer.Serialize(
            _processorInstanceList.Select(c =>
                $"{c.PluginId}/{c.Name} - {JsonSerializer.Serialize(c.TargetAdapterId)}"),
            UndefinedApp.SerializerOptions);
        FileIO.WriteFile(Path.Join(Environment.CurrentDirectory, "loaded_plugins.json"), pluginListText);
        _logger.LogInformation("Loaded plugins:{PluginList}", pluginListText);
        _logger.LogInformation("Loaded commands:{PluginList}", commandListText);
        _logger.LogInformation("Loaded message processors:{PluginList}", processorListText);
        IndexInstances();
    }

    private IPluginInstance? CreatePluginInstance(string pluginLibPath)
    {
        try
        {
            //Get Plugin Class
            Type? targetClass = Assembly
                .LoadFrom(pluginLibPath)
                .GetTypes()
                .ToList()
                .Find(type => type.BaseType?.FullName == "UndefinedBot.Core.Plugin.BasePlugin");
            if (targetClass is null)
            {
                _logger.LogWarning("Entry point not found in assembly {pluginLibPath}", pluginLibPath);
                return null;
            }

            _logger.LogTrace("Plugin class: {targetClass}", targetClass.FullName);
            //Create Plugin Class Instance to Invoke Initialize Method
            object? instance = Activator.CreateInstance(
                targetClass,
                [
                    new PluginDependencyCollection
                    {
                        LoggerFactory = provider.GetRequiredService<InternalILoggerFactory>(),
                        PluginConfig =
                            new ConfigProvider(
                                FileIO.ReadAsJson(
                                    Path.Join(Path.GetDirectoryName(pluginLibPath), "plugin.json")
                                ) ?? throw new FileNotFoundException("plugin.json")
                            )
                    }
                ]);
            if (instance is IPluginInstance targetPluginInstance)
            {
                _logger.LogTrace("Plugin instance created: {targetAdapterInstance}", targetPluginInstance.Id);
                return targetPluginInstance;
            }

            _logger.LogWarning("Fail to create instance from {targetClass}", targetClass.FullName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to load command from {pluginLibPath}", pluginLibPath);
        }

        return null;
    }

    private static string GetLibSuffix()
    {
        return "dll";
    }

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
    private void IndexInstances()
    {
        foreach (CommandInstance ci in _commandInstanceList)
        foreach (string tai in ci.TargetAdapterId)
        {
            if (_commandIndex.TryAdd(tai, [ci])) continue;

            _commandIndex[tai].Add(ci);
        }

        foreach (MessageProcessorInstance mpi in _processorInstanceList)
        foreach (string tai in mpi.TargetAdapterId)
        {
            if (_processorIndex.TryAdd(tai, [mpi])) continue;

            _processorIndex[tai].Add(mpi);
        }
    }

    public void Unload()
    {
        foreach (IPluginInstance pi in _pluginInstanceList) pi.Dispose();

        foreach (CommandInstance ci in _commandInstanceList) ci.Dispose();

        _pluginInstanceList.Clear();
        _commandInstanceList.Clear();
        _commandIndex.Clear();
    }

    public void Dispose()
    {
        foreach (IPluginInstance pi in _pluginInstanceList) pi.Dispose();

        _pluginInstanceList.Clear();
    }
}