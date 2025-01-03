using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Plugin;

namespace UndefinedBot.Net.Utils;

internal static class PluginLoader
{
    private static string PluginRoot => Path.Join(Program.GetProgramRoot(), "Plugins");
    private static string LibSuffix => GetLibSuffix();
    private static ILogger PluginInitializeLogger => new GeneralLogger("Plugin Load");
    private static readonly List<IPluginInstance> _pluginReference = [];
    private static readonly Dictionary<string, CommandProperties> _commandReference = [];
    private static readonly Dictionary<string, CommandInstance> _commandInstance = [];

    internal static List<IPluginInstance> LoadPlugins()
    {
        PluginInitializeLogger.Info("Start Loading Plugins");
        if (!Directory.Exists(PluginRoot))
        {
            Directory.CreateDirectory(PluginRoot);
            throw new FileNotFoundException("Plugins Folder Not Fount, Restart.");
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
            PluginConfigData? pluginConfigData = originJson.Deserialize<PluginConfigData>();
            if (originJson == null || pluginConfigData == null || !pluginConfigData.IsValid())
            {
                PluginInitializeLogger.Warn($"Plugin: <{pf}> Invalid plugin.json");
                continue;
            }

            pluginConfigData.Implement(originJson);
            string entryFile = $"{Path.Join(pf, pluginConfigData.EntryFile)}.{LibSuffix}";
            if (!File.Exists(entryFile))
            {
                PluginInitializeLogger.Warn($"Plugin: <{pf}> Binary EntryFile: <{entryFile}> Not Found");
                continue;
            }

            IPluginInstance? pluginInstance = LoadCommand(entryFile, pluginConfigData);
            if (pluginInstance == null)
            {
                continue;
            }

            foreach (var ci in pluginInstance.GetCommandInstance())
            {
                _commandInstance.TryAdd(ci.Name,ci);
            }

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
            FileIO.WriteAsJson(commandRefPath, pluginInstance.GetCommandInstance().Select(ci => ci.ExportToCommandProperties()));
            _pluginReference.Add(pluginInstance);
        }

        return _pluginReference;
    }

    private static IPluginInstance? LoadCommand(string pluginLibPath, PluginConfigData config)
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
                Activator.CreateInstance(targetClass,[config]) as IPluginInstance ??
                throw new TypeInitializationException(targetClass.FullName, null);
            targetClassInstance.Initialize();
            List<CommandInstance> instances = targetClassInstance.GetCommandInstance();
            string id = targetClassInstance.Id;
            //Register Command
            foreach (CommandInstance commandInstance in instances)
            {
                CommandEventBus.RegisterCommandEventHandler(async (invokeProperties, commandSource) =>
                {
                    if (commandInstance.TargetAdapterId != invokeProperties.AdapterId)
                    {
                        return;
                    }

                    if (commandInstance.Name != invokeProperties.Command &&
                        !commandInstance.CommandAlias.Contains(invokeProperties.Command))
                    {
                        return;
                    }

                    CommandContext ctx = new(commandInstance.Name, id, invokeProperties);
                    ctx.Logger.Info("Command Triggered");
                    //While any node matches the token,control flow will execute this node and throw CommandFinishException to exit.
                    try
                    {
                        ICommandResult result = await commandInstance.Run(ctx, commandSource, invokeProperties.Tokens);
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
                        ctx.Logger.Error(ex,"Command Failed");
                    }

                    ctx.Logger.Info("Command Completed");
                    //uApi.FinishEvent.Trigger();
                });
                //uApi.Logger.Info($"Successful Load Command <{commandInstance.Name}>");
            }
            return targetClassInstance;
        }
        catch (TypeLoadException tle)
        {
            PluginInitializeLogger.Error(tle,"Unable to Find Specific Plugin Class");
        }
        catch (TypeInitializationException tie)
        {
            PluginInitializeLogger.Error(tie,"Unable to Create Plugin Instance");
        }
        catch (MethodAccessException)
        {
            PluginInitializeLogger.Error("Unable to Find Specific Plugin Initialize Method");
        }
        catch (Exception ex)
        {
            PluginInitializeLogger.Error(ex,$"Unable to Load Command From {pluginLibPath}");
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
        if (!FileIO.EnsurePath(Path.Join(Program.GetProgramRoot(), "CommandReference")))
        {
            throw new TargetException("CommandReference Folder Not Exist");
        }

        foreach (
            var p in Directory
                .GetFiles(Path.Join(Program.GetProgramRoot(), "CommandReference"))
                .Select(cfp =>
                    (FileIO.ReadAsJson<List<CommandProperties>>(cfp) ?? [])
                    .ToDictionary(k =>
                        $"{Path.GetFileName(cfp).Split(".")[0]}:{k.Name}", v => v)
                )
                .SelectMany(item => item)
                .Where(item => item.Value.IsValid())
        )
        {
            _commandReference[p.Key] = p.Value;
        }

        return _commandReference;
    }
}
