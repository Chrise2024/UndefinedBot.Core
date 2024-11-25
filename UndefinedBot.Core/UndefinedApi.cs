using System.Reflection;
using Newtonsoft.Json;
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
        /// <summary>
        /// Submit Command After Register
        /// </summary>
        public void SubmitCommand()
        {
            //while command is triggered, control flow will travel all child nodes by deep-first.
            //If none child node in node's all child,this node will execute itself.
            //one node is only accessible to args the same level as itself or more front.
            List<CommandInstance> commandRef = [];
            string commandRefPath = Path.Join(RootPath, "CommandReference", $"{PluginName}.reference.json");
            foreach (var commandInstance in _commandInstances)
            {
                CommandHandler.TriggerEvent.OnCommand += async (CallingProperty cp,List<string> tokens) => {
                    if (commandInstance.Name.Equals(cp.Command) || commandInstance.CommandAlias.Contains(cp.Command))
                    {
                        CommandContext ctx = new(commandInstance.Name, this, cp);
                        ctx.Logger.Info("Command Triggered");
                        //While any node matches the token,control flow will execute this node and throw CommandFinishException to exit.
                        try
                        {
                            await commandInstance.Run(ctx,tokens);
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
}
