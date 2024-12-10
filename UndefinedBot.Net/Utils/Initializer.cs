using System.Reflection;
using System.Text.Json.Serialization;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;

namespace UndefinedBot.Net.Utils;

internal abstract class Initializer
{
    private static readonly string s_pluginRoot = Path.Join(Program.GetProgramRoot(),"Plugins");

    private static readonly GeneralLogger s_initLogger = new("Initialize");
    internal static List<PluginProperty> LoadPlugins()
    {
        if (!Directory.Exists(s_pluginRoot))
        {
            Directory.CreateDirectory(s_pluginRoot);
            return [];
        }

        string[] pluginFolders = Directory.GetDirectories(s_pluginRoot);
        List<PluginProperty> pluginRef = [];
        foreach (string pf in pluginFolders)
        {
            string pluginPropFile = Path.Join(pf, "plugin.json");
            string pluginUcFile = Path.Join(pf, "UndefinedBot.Core.dll");
            if (!File.Exists(pluginPropFile))
            {
                continue;
            }

            PluginProperty pluginProperty = FileIO.ReadAsJson<PluginProperty>(pluginPropFile);
            if (!pluginProperty.Equals(default(PluginProperty)))
            {
                s_initLogger.Warn($"Plugin: <{pf}> Invalid plugin.json");
                continue;
            }

            string entryFile = Path.Join(pf, pluginProperty.EntryFile);
            if (!File.Exists(entryFile))
            {
                s_initLogger.Warn($"Plugin: <{pf}> EntryFile: <{entryFile}> Not Found");
                continue;
            }

            FileIO.SafeDeleteFile(pluginUcFile);
            string pluginCachePath = Path.Join(Program.GetProgramCache(), pluginProperty.Name);
            FileIO.EnsurePath(pluginCachePath);
            foreach (string cf in Directory.GetFiles(pluginCachePath))
            {
                FileIO.SafeDeleteFile(cf);
            }

            object? pInstance = InitPlugin(entryFile, pluginProperty.Name);
            if (pInstance != null)
            {
                pluginProperty.Instance = pInstance;
                pluginRef.Add(pluginProperty);
            }
            else
            {
                s_initLogger.Warn($"Plugin: <{pf}> load failed");
            }
        }
        return pluginRef;
    }
    public static Dictionary<string, CommandProperty> GetCommandReference()
    {
        if (!FileIO.EnsurePath(Path.Join(Program.GetProgramRoot(), "CommandReference")))
        {
            throw new TargetException("CommandReference Not Exist");
        }
        return Directory
            .GetFiles(Path.Join(Program.GetProgramRoot(),"CommandReference"))
            .Select(cfp => FileIO.ReadAsJson<List<CommandProperty>>(cfp) ?? [])
            .SelectMany(item => item)
            .Where(item => !item.Equals(default(CommandProperty)))
            .ToDictionary(item => item.Name, item => item);
    }
    private static object? InitPlugin(string pluginDllPath, string pluginName)
    {
        try
        {
            return Activator.CreateInstance(Assembly
                .LoadFrom(pluginDllPath)
                .GetTypes()
                .FirstOrDefault(t =>
                    t.IsClass &&
                    t.GetCustomAttributes()
                        .Any(item => item.ToString()?.Equals("UndefinedBot.Core.PluginAttribute") ?? false)) ?? throw new Exception(),
                [pluginName]
                );
        }
        catch
        {
            return null;
        }
    }
}
internal struct PluginProperty
{
    [JsonPropertyName("name")] public string Name;
    [JsonPropertyName("description")] public string Description;
    [JsonPropertyName("entry_file")] public string EntryFile;
    [JsonPropertyName("entry_point")] public string? EntryPoint;
    [JsonIgnore] public object? Instance;
}
