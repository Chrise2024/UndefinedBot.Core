using System.Reflection;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.Arguments.ArgumentType;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core
{
    public delegate void CommandFinishHandler();
    public delegate Task CommandActionHandler(CommandContext commandContext);
    public class CommandFinishEvent
    {
        public event CommandFinishHandler? OnCommandFinish;

        public void Trigger()
        {
            OnCommandFinish?.Invoke();
        }
    }
    internal class Core
    {
        private static readonly string s_programRoot = Environment.CurrentDirectory;

        private static readonly ConfigManager s_mainConfigManager = new();
        public static ConfigManager GetConfigManager()
        {
            return s_mainConfigManager;
        }
        public static string GetCoreRoot()
        {
            return s_programRoot;
        }
    }
    public class UndefinedAPI
    {
        public readonly string PluginName;
        public readonly string PluginPath;
        public readonly Logger Logger;
        public readonly HttpApi Api;
        public readonly HttpRequest Request;
        public readonly ConfigManager Config;
        public readonly string RootPath;
        public readonly string CachePath;
        public readonly CommandFinishEvent FinishEvent;
        public readonly CacheManager Cache;
        private readonly List<CommandInstance> _commandInstances = [];
        public UndefinedAPI(string pluginName)
        {
            PluginName = pluginName;
            PluginPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) ?? Path.Join(Environment.CurrentDirectory,"Plugins",pluginName);
            Logger = new(pluginName);
            Api = new(Core.GetConfigManager().GetHttpPostUrl());
            Request = new();
            Config = new();
            RootPath = Environment.CurrentDirectory;
            CachePath = Path.Join(Core.GetCoreRoot(), "Cache", pluginName);
            FinishEvent = new();
            Cache = new(pluginName, CachePath, FinishEvent);
        }
        public void SubmitCommand()
        {
            List<CommandInstance> commandRef = [];
            string commandRefPath = Path.Join(RootPath, "CommandReference", $"{PluginName}.reference.json");
            foreach (var commandInstance in _commandInstances)
            {
                CommandHandler.TriggerEvent.OnCommand += async (CallingProperty cp,List<string> tokens) => {
                    if (commandInstance.Name.Equals(cp.Command) || commandInstance.CommandAlias.Contains(cp.Command))
                    {
                        Dictionary<string, string> argumentRef = [];
                        RecurseNode(argumentRef,commandInstance.RootNode,tokens,0);
                        CommandContext ctx = new(commandInstance.Name,tokens , this, cp, argumentRef);
                        ctx.Logger.Info("Command Triggered");
                        try
                        {
                            await commandInstance.RootNode.ExecuteSelf(ctx);
                            ctx.Logger.Info("Command Failed");
                        }
                        catch (ArgumentInvalidException ex)
                        {
                            ctx.Logger.Info($"Invalid Argument: {ex.Message}");
                        }
                        catch (CommandFinishException ex)
                        {
                            ctx.Logger.Info(ex.Message);
                        }
                        FinishEvent.Trigger();
                    }
                };
                commandRef.Add(commandInstance);
                this.Logger.Info($"Successful Load Command <{commandInstance.Name}>");
            }
            FileIO.WriteAsJson(commandRefPath, commandRef);
        }
        public CommandInstance RegisterCommand(string commandName)
        {
            CommandInstance ci = new(commandName);
            _commandInstances.Add(ci);
            return ci;
        }

        private void RecurseNode(Dictionary<string, string> ar, ICommandNode cNode,List<string> tokens, int index)
        {
            if (index >= tokens.Count)
            {
                return;
            }
            foreach (ICommandNode node in cNode.Child)
            {
                ar[node.NodeName] = tokens[index];
                RecurseNode(ar, node, tokens, index + 1);
            }
        }
    }
}
