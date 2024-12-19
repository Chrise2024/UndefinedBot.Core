using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using UndefinedBot.Core;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Command.CommandResult;

namespace UndefinedBot.Net.Utils;

//To-do: Move Initializer Into Core
internal abstract class Initializer
{
    private static readonly PluginMetaProperties s_defaultPluginMeta = new();
    private static readonly CommandMetaProperties s_defaultCommandMeta = new();
    private static readonly string s_pluginRoot = Path.Join(Program.GetProgramRoot(), "Plugins");
    private static readonly string s_libSuffix = GetLibSuffix();
    private static readonly GeneralLogger s_initLogger = new("Initialize");

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

    internal static (List<PluginMetaProperties>, Dictionary<string, CommandInstance>) LoadPlugins()
    {
        if (string.IsNullOrEmpty(s_libSuffix))
        {
            throw new PlatformNotSupportedException();
        }

        s_initLogger.Info("Start Loading Plugins");
        if (!Directory.Exists(s_pluginRoot))
        {
            Directory.CreateDirectory(s_pluginRoot);
            return ([], []);
        }

        string[] pluginFolders = Directory.GetDirectories(s_pluginRoot);
        List<PluginMetaProperties> pluginRef = [];
        Dictionary<string, CommandInstance> commandInstance = [];
        foreach (string pf in pluginFolders)
        {
            string pluginPropertiesFile = Path.Join(pf, "plugin.json");
            //Remove SDK Lib to Let Plugin Call Main Program's UndefinedBot.Core Lib
            string pluginSdkFile = Path.Join(pf, $"UndefinedBot.Core.{s_libSuffix}");
            FileIO.SafeDeleteFile(pluginSdkFile);

            if (!File.Exists(pluginPropertiesFile))
            {
                s_initLogger.Warn($"Plugin: <{pf}> Not Have plugin.json");
                continue;
            }

            PluginMetaProperties? pluginMetaProperties = FileIO.ReadAsJson<PluginMetaProperties>(pluginPropertiesFile);
            if (pluginMetaProperties == null || pluginMetaProperties.Equals(s_defaultPluginMeta))
            {
                s_initLogger.Warn($"Plugin: <{pf}> Invalid plugin.json");
                continue;
            }

            string entryFile = $"{Path.Join(pf, pluginMetaProperties.EntryFile)}.{s_libSuffix}";
            if (!File.Exists(entryFile))
            {
                s_initLogger.Warn($"Plugin: <{pf}> Binary EntryFile: <{entryFile}> Not Found");
                continue;
            }

            string pluginCachePath = Path.Join(Program.GetProgramCache(), pluginMetaProperties.Name);
            FileIO.EnsurePath(pluginCachePath);
            foreach (string cf in Directory.GetFiles(pluginCachePath))
            {
                //Clear Remaining Cache
                FileIO.SafeDeleteFile(cf);
            }

            Dictionary<string, CommandInstance> cmd = LoadCommand(entryFile, pluginMetaProperties.Name);
            foreach (var pair in cmd)
            {
                commandInstance.TryAdd(pair.Key, pair.Value);
            }

            string commandRefPath = Path.Join(
                Environment.CurrentDirectory, "CommandReference",
                $"{pluginMetaProperties.Name}.reference.json"
                );
            FileIO.WriteAsJson(commandRefPath, cmd.Select(p => p.Value.ExportToCommandProperties()));
            pluginRef.Add(pluginMetaProperties);
            //Console.WriteLine($"{pf} Loaded");
            //Console.WriteLine();
        }

        return (pluginRef, commandInstance);
    }

    public static Dictionary<string, CommandMetaProperties> GetCommandReference()
    {
        s_initLogger.Info("Extracting Command References");
        if (!FileIO.EnsurePath(Path.Join(Program.GetProgramRoot(), "CommandReference")))
        {
            throw new TargetException("CommandReference Folder Not Exist");
        }

        return Directory
            .GetFiles(Path.Join(Program.GetProgramRoot(), "CommandReference"))
            .Select(cfp =>
                (FileIO.ReadAsJson<List<CommandMetaProperties>>(cfp) ?? [])
                .ToDictionary(k =>
                    $"{Path.GetFileName(cfp).Split(".")[0]}:{k.Name}", v => v)
                )
            .SelectMany(item => item)
            .Where(item => !item.Value.Equals(s_defaultCommandMeta))
            .ToDictionary(k => k.Key,v=>v.Value);
    }

    private static Dictionary<string, CommandInstance> LoadCommand(string pluginLibPath, string pluginName)
    {
        UndefinedApi uApi = new(pluginName);
        try
        {
            //Get Plugin Class
            Type targetClass = Assembly
                .LoadFrom(pluginLibPath)
                .GetTypes()
                .ToList()
                .Find(type =>
                    type.GetInterfaces().Any(item =>
                        item.FullName == "UndefinedBot.Core.Registry.IPluginInitializer")
                ) ?? throw new TypeLoadException(pluginName);
            //Create Plugin Class Instance to Invoke Initialize Method
            object targetClassInstance =
                Activator.CreateInstance(targetClass) ??
                throw new TypeInitializationException(targetClass.FullName, null);
            //Get and Invoke Initialize Method
            MethodInfo methodInfo = targetClass.GetMethod("Initialize") ??
                                    throw new MethodAccessException();
            methodInfo.Invoke(targetClassInstance, [uApi]);
            //Register Command
            foreach (CommandInstance commandInstance in uApi._commandInstances)
            {
                CommandEventBus.RegisterCommandEventHandler(async (invokeProperties, commandSource) =>
                {
                    if (commandInstance.Name != invokeProperties.Command &&
                        !commandInstance.CommandAlias.Contains(invokeProperties.Command))
                    {
                        return;
                    }

                    CommandContext ctx = new(commandInstance.Name, uApi, invokeProperties);
                    ctx.Logger.Info("Command Triggered");
                    //While any node matches the token,control flow will execute this node and throw CommandFinishException to exit.
                    try
                    {
                        ICommandResult result =
                            await commandInstance.Run(ctx, commandSource, invokeProperties.Tokens);
                        switch (result)
                        {
                            case CommandSuccess:
                                //ignore
                                break;
                            case InvalidArgument iae:
                                ctx.Logger.Error(
                                    $"Invalid argument: {iae.ErrorToken}, require {JsonSerializer.Serialize(iae.RequiredType)}");
                                break;
                            case TooLessArgument tae:
                                ctx.Logger.Error(
                                    $"To less arguments, require {JsonSerializer.Serialize(tae.RequiredType)}");
                                break;
                            case PermissionDenied pde:
                                ctx.Logger.Error(
                                    $"Not enough permission: {pde.CurrentPermission} at {pde.CurrentNode}, require {pde.RequiredPermission}");
                                break;
                        }
                    }
                    catch (CommandAbortException)
                    {
                        ctx.Logger.Error($"Command Execute Aborted");
                    }
                    catch (CommandSyntaxException cse)
                    {
                        ctx.Logger.Error($"Node {cse.CurrentNode} Not Implemented");
                    }
                    catch (Exception ex)
                    {
                        ctx.Logger.Error("Command Failed");
                        ctx.Logger.Error(ex.Message);
                        ctx.Logger.Error(ex.StackTrace ?? "");
                    }

                    ctx.Logger.Info("Command Completed");
                    uApi.FinishEvent.Trigger();
                });
                uApi.Logger.Info($"Successful Load Command <{commandInstance.Name}>");
            }
            Dictionary<string, CommandInstance> t = uApi._commandInstances.ToDictionary(k => k.Name, v => v);
            uApi._commandInstances.Clear();
            return t;
        }
        catch (TypeLoadException tle)
        {
            s_initLogger.Error($"Unable to Find Specific Plugin Class: {tle.Message}");
        }
        catch (TypeInitializationException tie)
        {
            s_initLogger.Error($"Unable to Create Plugin Instance: {tie.Message}");
        }
        catch (MethodAccessException)
        {
            s_initLogger.Error($"Unable to Find Specific Plugin Initialize Method");
        }
        catch (Exception)
        {
            s_initLogger.Error($"Unable to Load Command From {pluginName}");
        }

        return [];
    }
}
[Serializable] public class PluginMetaProperties : IEquatable<PluginMetaProperties>
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("entry_file")] public string EntryFile { get; set; } = "";
    public bool Equals(PluginMetaProperties? other)
    {
        return Name == other?.Name &&
               Description == other.Description &&
               EntryFile == other.EntryFile;
    }
}
