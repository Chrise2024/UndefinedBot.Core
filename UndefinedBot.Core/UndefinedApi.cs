using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;

[assembly:InternalsVisibleTo("UndefinedBot.Net")]
[assembly:InternalsVisibleTo("UndefinedBot.Core.Test")]
namespace UndefinedBot.Core;

public delegate void CommandFinishHandler();
public delegate Task CommandActionHandler(CommandContext ctx);
public class CommandFinishEvent
{
    public event CommandFinishHandler? OnCommandFinish;

    public void Trigger()
    {
        OnCommandFinish?.Invoke();
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class PluginAttribute : Attribute;
public class UndefinedApi(string pluginName)
{
    public  string PluginName => pluginName;
    public string RootPath => Environment.CurrentDirectory;
    public string PluginPath => Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) ?? throw new DllNotFoundException("Get Plugin Assembly Failed");
    public string CachePath => Path.Join(RootPath, "Cache", pluginName);
    public GeneralLogger Logger => new(pluginName);
    public HttpApi Api => new(ConfigData.GetHttpPostUrl());
    public HttpRequest Request => new();
    public Config ConfigData => ConfigManager.GetConfig();
    public CommandFinishEvent FinishEvent => new();
    public CacheManager Cache => new(pluginName, CachePath, FinishEvent);
    internal readonly List<CommandInstance> _commandInstances = [];
    /// <summary>
    /// Submit Command After Register
    /// </summary>
    [Obsolete("Now Need not to Submit",true)]public void SubmitCommand()
    {
        //while command is triggered, control flow will travel all child nodes by deep-first.
        //If none child node in node's all child,this node will execute itself.
        //one node is only accessible to args the same level as itself or more front.
        List<CommandInstance> commandRef = [];
        string commandRefPath = Path.Join(RootPath, "CommandReference", $"{PluginName}.reference.json");
        foreach (var commandInstance in _commandInstances)
        {
            CommandHandler.RegisterCommandEventHandler(async (cp,tokens) => {
                if (commandInstance.Name == cp.Command || commandInstance.CommandAlias.Contains(cp.Command))
                {
                    CommandContext ctx = new(commandInstance.Name, this, cp);
                    ctx.Logger.Info("Command Triggered");
                    //While any node matches the token,control flow will execute this node and throw CommandFinishException to exit.
                    try
                    {
                        ICommandResult result = await commandInstance.Run(ctx, tokens);
                        switch (result)
                        {
                            case CommandSuccess:
                                //ignore
                                break;
                            case InvalidArgument iae:
                                ctx.Logger.Error($"Invalid argument: {iae.ErrorToken}, require {JsonSerializer.Serialize(iae.RequiredType)}");
                                break;
                            case TooLessArgument tae:
                                ctx.Logger.Error($"To less arguments, require {JsonSerializer.Serialize(tae.RequiredType)}");
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
                    FinishEvent.Trigger();
                }
            });
            commandRef.Add(commandInstance);
            Logger.Info($"Successful Load Command <{commandInstance.Name}>");
        }
        FileIO.WriteAsJson(commandRefPath, commandRef);
    }
    /// <summary>
    /// Register Command
    /// </summary>
    /// <param name="commandName">
    /// Command Name to be Called
    /// </param>
    /// <returns>
    /// CommandInstance
    /// </returns>
    public CommandInstance RegisterCommand(string commandName)
    {
        CommandInstance ci = new(commandName);
        _commandInstances.Add(ci);
        return ci;
    }
}
