using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Plugin;

namespace UndefinedBot.Net.Utils;

internal sealed class PluginService(ILogger<PluginService> logger) : IDisposable
{
    private static string PluginRoot => Path.Join(Program.GetProgramRoot(), "Plugins");
    private static string LibSuffix => GetLibSuffix();
    private readonly List<IPluginInstance> _pluginInstanceList = [];
    private ILogger<PluginService> Logger => logger;

    public List<IPluginInstance> LoadPlugins()
    {

        List<CommandInstance> commandInstanceList = [];
        Logger.LogInformation("Start loading plugins");
        if (!Directory.Exists(PluginRoot))
        {
            Directory.CreateDirectory(PluginRoot);
            Logger.LogWarning("Plugins folder not fount, creating Plugins folder.");
            return [];
        }

        string[] pluginFolders = Directory.GetDirectories(PluginRoot);
        foreach (string pf in pluginFolders)
        {
            string pluginPropertiesFile = Path.Join(pf, "plugin.json");
            //Remove SDK Lib to Let Plugin Call Main Program's UndefinedBot.Core Lib
            string pluginSdkFile = Path.Join(pf, $"UndefinedBot.Core.{LibSuffix}");
            FileIO.SafeDeleteFile(pluginSdkFile);

            if (!File.Exists(pluginPropertiesFile))
            {
                Logger.LogWarning("<{pf}> not have plugin.json", pf);
                continue;
            }

            JsonNode? originJson = FileIO.ReadAsJson(pluginPropertiesFile);
            string? ef = originJson?["EntryFile"]?.GetValue<string>();
            if (originJson is null || ef is null)
            {
                Logger.LogWarning("<{pf}> has invalid plugin.json", pf);
                continue;
            }

            string entryFile = $"{Path.Join(pf, ef)}.{LibSuffix}";
            if (!File.Exists(entryFile))
            {
                Logger.LogWarning("Binary EntryFile: <{entryFile}> Not Found", entryFile);
                continue;
            }

            IPluginInstance? pluginInstance = CreatePluginInstance(entryFile);
            if (pluginInstance is null)
            {
                continue;
            }

            commandInstanceList.AddRange(pluginInstance.GetCommandInstance());

            string pluginCachePath = Path.Join(Program.GetProgramCache(), pluginInstance.Id);
            FileIO.EnsurePath(pluginCachePath);
            foreach (string cf in Directory.GetFiles(pluginCachePath))
            {
                //Clear Remaining Cache
                FileIO.SafeDeleteFile(cf);
            }
            _pluginInstanceList.Add(pluginInstance);
        }

        CommandManager.UpdateCommandInstances(commandInstanceList);
        return _pluginInstanceList;
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
                Logger.LogWarning("Entry point not found in assembly {pluginLibPath}", pluginLibPath);
                return null;
            }

            //Create Plugin Class Instance to Invoke Initialize Method
            if (Activator.CreateInstance(targetClass) is IPluginInstance targetPluginInstance)
            {
                Logger.LogTrace("Adapter instance created: {targetAdapterInstance}", targetPluginInstance.Id);
                targetPluginInstance.Initialize();
                return targetPluginInstance;
            }

            Logger.LogWarning("Fail to create instance from {targetClass}", targetClass.FullName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unable to Load Command From {pluginLibPath}", pluginLibPath);
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
    //for Help command
    public void Unload()
    {
        CommandManager.DisposeCommandInstance();
        foreach (var pi in _pluginInstanceList)
        {
            pi.Dispose();
        }
        _pluginInstanceList.Clear();
    }

    public void Dispose()
    {
        Unload();
        _pluginInstanceList.Clear();
    }
}