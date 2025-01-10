using System.Reflection;
using System.Text.Json.Nodes;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Plugin;

namespace UndefinedBot.Net.Utils;

internal static class PluginLoader
{
    private static string PluginRoot => Path.Join(Program.GetProgramRoot(), "Plugins");
    private static string LibSuffix => GetLibSuffix();
    private static ILogger PluginInitializeLogger => new BaseLogger(["Init", "Load Plugin"]);

    internal static List<IPluginInstance> LoadPlugins()
    {
        List<CommandInstance> commandInstanceList = [];
        List<IPluginInstance> pluginInstanceList = [];
        PluginInitializeLogger.Info("Start Loading Plugins");
        if (!Directory.Exists(PluginRoot))
        {
            Directory.CreateDirectory(PluginRoot);
            PluginInitializeLogger.Warn("Plugins Folder Not Fount, Creating Adapters Folder.");
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
                PluginInitializeLogger.Warn($"Plugin: <{pf}> Not Have plugin.json");
                continue;
            }

            JsonNode? originJson = FileIO.ReadAsJson(pluginPropertiesFile);
            string? ef = originJson?["EntryFile"]?.GetValue<string>();
            if (originJson is null || ef is null)
            {
                PluginInitializeLogger.Warn($"Plugin: <{pf}> Invalid plugin.json");
                continue;
            }

            string entryFile = $"{Path.Join(pf, ef)}.{LibSuffix}";
            if (!File.Exists(entryFile))
            {
                PluginInitializeLogger.Warn($"Plugin: <{pf}> Binary EntryFile: <{entryFile}> Not Found");
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

            string commandRefPath = Path.Join(
                Environment.CurrentDirectory, "CommandReference",
                $"{pluginInstance.Id}.reference.json"
            );
            FileIO.WriteAsJson(commandRefPath,
                pluginInstance.GetCommandInstance().Select(ci => ci.ExportToCommandProperties()));
            pluginInstanceList.Add(pluginInstance);
        }

        CommandInvokeManager.UpdateCommandInstances(commandInstanceList);
        return pluginInstanceList;
    }

    private static IPluginInstance? CreatePluginInstance(string pluginLibPath)
    {
        try
        {
            //Get Plugin Class
            Type targetClass = Assembly
                                   .LoadFrom(pluginLibPath)
                                   .GetTypes()
                                   .ToList()
                                   .Find(type => type.BaseType?.FullName == "UndefinedBot.Core.Plugin.BasePlugin")
                               ?? throw new TypeAccessException("Plugin Class Not Fount");
            //Create Plugin Class Instance to Invoke Initialize Method
            IPluginInstance targetClassInstance =
                Activator.CreateInstance(targetClass) as IPluginInstance ??
                throw new TypeInitializationException(targetClass.FullName, null);
            targetClassInstance.Initialize();
            return targetClassInstance;
        }
        catch (TargetInvocationException tie)
        {
            PluginInitializeLogger.Error(tie.InnerException, "Plugin's Constructor Occurs Exception");
        }
        catch (TypeLoadException tle)
        {
            PluginInitializeLogger.Error(tle, "Unable to Find Specific Plugin Class");
        }
        catch (TypeInitializationException tie)
        {
            PluginInitializeLogger.Error(tie, "Unable to Create Plugin Instance");
        }
        catch (MethodAccessException)
        {
            PluginInitializeLogger.Error("Unable to Find Specific Plugin Initialize Method");
        }
        catch (Exception ex)
        {
            PluginInitializeLogger.Error(ex, $"Unable to Load Command From {pluginLibPath}");
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
    public static Dictionary<string, CommandProperties> GetCommandReference()
    {
        PluginInitializeLogger.Info("Extracting Command References");
        if (FileIO.EnsurePath(Path.Join(Program.GetProgramRoot(), "CommandReference")))
        {
            return Directory
                .GetFiles(Path.Join(Program.GetProgramRoot(), "CommandReference"))
                .Select(cfp =>
                    (FileIO.ReadAsJson<List<CommandProperties>>(cfp) ?? [])
                    .ToDictionary(k =>
                            $"{Path.GetFileName(cfp).Split(".")[0]}:{k.Name}", v => v
                    )
                )
                .SelectMany(item => item)
                .Where(item => item.Value.IsValid())
                .ToDictionary(k => k.Key, v => v.Value);
        }

        PluginInitializeLogger.Warn("CommandReference Folder Not Exist");
        return [];
    }
}