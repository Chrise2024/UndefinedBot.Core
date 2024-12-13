using System.Reflection;
using System.Text.Json.Serialization;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;

namespace UndefinedBot.Net.Utils;

internal abstract class Initializer
{
    private static readonly string s_pluginRoot = Path.Join(Program.GetProgramRoot(),"Plugins");

    private static readonly GeneralLogger s_initLogger = new("Initialize");
    internal static List<PluginProperties> LoadPlugins()
    {
        if (!Directory.Exists(s_pluginRoot))
        {
            Directory.CreateDirectory(s_pluginRoot);
            return [];
        }

        string[] pluginFolders = Directory.GetDirectories(s_pluginRoot);
        List<PluginProperties> pluginRef = [];
        foreach (string pf in pluginFolders)
        {
            string pluginPropFile = Path.Join(pf, "plugin.json");
            string pluginUcFile = Path.Join(pf, "UndefinedBot.Core.dll");
            if (!File.Exists(pluginPropFile))
            {
                continue;
            }

            PluginProperties pluginProperties = FileIO.ReadAsJson<PluginProperties>(pluginPropFile);
            if (!pluginProperties.Equals(default))
            {
                s_initLogger.Warn($"Plugin: <{pf}> Invalid plugin.json");
                continue;
            }

            string entryFile = Path.Join(pf, pluginProperties.EntryFile);
            if (!File.Exists(entryFile))
            {
                s_initLogger.Warn($"Plugin: <{pf}> EntryFile: <{entryFile}> Not Found");
                continue;
            }

            FileIO.SafeDeleteFile(pluginUcFile);
            string pluginCachePath = Path.Join(Program.GetProgramCache(), pluginProperties.Name);
            FileIO.EnsurePath(pluginCachePath);
            foreach (string cf in Directory.GetFiles(pluginCachePath))
            {
                FileIO.SafeDeleteFile(cf);
            }

            object? pInstance = CreatePluginInstance(entryFile, pluginProperties.Name);
            if (pInstance != null)
            {
                pluginProperties.Instance = pInstance;
                pluginRef.Add(pluginProperties);
            }
            else
            {
                s_initLogger.Warn($"Plugin: <{pf}> load failed");
            }
        }
        return pluginRef;
    }
    public static Dictionary<string, CommandProperties> GetCommandReference()
    {
        if (!FileIO.EnsurePath(Path.Join(Program.GetProgramRoot(), "CommandReference")))
        {
            throw new TargetException("CommandReference Not Exist");
        }
        return Directory
            .GetFiles(Path.Join(Program.GetProgramRoot(),"CommandReference"))
            .Select(cfp => FileIO.ReadAsJson<List<CommandProperties>>(cfp) ?? [])
            .SelectMany(item => item)
            .Where(item => !item.Equals(default(CommandProperties)))
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
internal struct PluginProperties : IEquatable<PluginProperties>
{
    [JsonPropertyName("name")] public string Name;
    [JsonPropertyName("description")] public string Description;
    [JsonPropertyName("entry_file")] public string EntryFile;
    [JsonPropertyName("entry_point")] public string? EntryPoint;
    [JsonIgnore] public object? Instance;
    [JsonIgnore] private static PluginProperties _defaultValue = default;

    public bool Equals(PluginProperties other)
    {
        return Name == other.Name &&
               Description == other.Description &&
               EntryFile == other.EntryFile;
    }
}
