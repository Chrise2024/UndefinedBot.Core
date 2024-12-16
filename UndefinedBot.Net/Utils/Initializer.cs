using System.Reflection;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;

namespace UndefinedBot.Net.Utils;

internal abstract class Initializer
{
    private static readonly PluginMetaProperties s_defaultPluginMeta = new();
    private static readonly CommandMetaProperties s_defaultCommandMeta = new();
    private static readonly string s_pluginRoot = Path.Join(Program.GetProgramRoot(),"Plugins");

    private static readonly GeneralLogger s_initLogger = new("Initialize");
    internal static List<PluginMetaProperties> LoadPlugins()
    {
        s_initLogger.Info("Start Loading Plugins");
        if (!Directory.Exists(s_pluginRoot))
        {
            Directory.CreateDirectory(s_pluginRoot);
            return [];
        }

        string[] pluginFolders = Directory.GetDirectories(s_pluginRoot);
        List<PluginMetaProperties> pluginRef = [];
        foreach (string pf in pluginFolders)
        {
            string pluginPropFile = Path.Join(pf, "plugin.json");
            string pluginUcFile = Path.Join(pf, "UndefinedBot.Core.dll");
            if (!File.Exists(pluginPropFile))
            {
                continue;
            }

            PluginMetaProperties? pluginMetaProperties = FileIO.ReadAsJson<PluginMetaProperties>(pluginPropFile);
            if (pluginMetaProperties == null || pluginMetaProperties.Equals(s_defaultPluginMeta))
            {
                s_initLogger.Warn($"Plugin: <{pf}> Invalid plugin.json");
                continue;
            }

            string entryFile = Path.Join(pf, pluginMetaProperties.EntryFile);
            if (!File.Exists(entryFile))
            {
                s_initLogger.Warn($"Plugin: <{pf}> EntryFile: <{entryFile}> Not Found");
                continue;
            }

            FileIO.SafeDeleteFile(pluginUcFile);
            string pluginCachePath = Path.Join(Program.GetProgramCache(), pluginMetaProperties.Name);
            FileIO.EnsurePath(pluginCachePath);
            foreach (string cf in Directory.GetFiles(pluginCachePath))
            {
                FileIO.SafeDeleteFile(cf);
            }
            object? pInstance = CreatePluginInstance(entryFile, pluginMetaProperties.Name);
            if (pInstance != null)
            {
                pluginMetaProperties.Instance = pInstance;
                pluginRef.Add(pluginMetaProperties);
            }
            else
            {
                s_initLogger.Warn($"Plugin: <{pf}> load failed");
            }
        }
        return pluginRef;
    }
    public static Dictionary<string, CommandMetaProperties> GetCommandReference()
    {
        s_initLogger.Info("Extracting Command References");
        if (!FileIO.EnsurePath(Path.Join(Program.GetProgramRoot(), "CommandReference")))
        {
            throw new TargetException("CommandReference Not Exist");
        }
        return Directory
            .GetFiles(Path.Join(Program.GetProgramRoot(),"CommandReference"))
            .Select(cfp => FileIO.ReadAsJson<List<CommandMetaProperties>>(cfp) ?? [])
            .SelectMany(item => item)
            .Where(item => !item.Equals(s_defaultCommandMeta))
            .ToDictionary(item => item.Name, item => item);
    }
    private static object? CreatePluginInstance(string pluginDllPath, string pluginName)
    {
        try
        {
            return Activator.CreateInstance(
                Assembly
                .LoadFrom(pluginDllPath)
                .GetTypes()
                .ToList()
                .Find(t =>
                    t.IsClass &&
                    t.GetCustomAttributes()
                        .Any(item => item.ToString() == "UndefinedBot.Core.PluginAttribute")
                    ) ?? throw new Exception(),
                [pluginName]
                );
        }
        catch
        {
            return null;
        }
    }
}
