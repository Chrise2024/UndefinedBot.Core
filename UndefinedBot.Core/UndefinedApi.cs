﻿using System.Reflection;
using Newtonsoft.Json;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core
{
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
    public sealed class PluginAttribute : Attribute
    {
        public PluginAttribute() { }
    }
    [Obsolete("wasted",true)]
    internal abstract class Core
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
    public class UndefinedApi
    {
        public readonly string PluginName;
        public readonly string PluginPath;
        public readonly Logger Logger;
        public readonly HttpApi Api;
        public readonly HttpRequest Request;
        public readonly Config Config;
        public readonly string RootPath;
        public readonly string CachePath;
        public readonly CommandFinishEvent FinishEvent;
        public readonly CacheManager Cache;
        private readonly List<CommandInstance> _commandInstances = [];
        public UndefinedApi(string pluginName)
        {
            PluginName = pluginName;
            //Plugin call core sdk assembly
            PluginPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) ?? throw new DllNotFoundException("Get Plugin Assembly Failed");
            Logger = new(pluginName);
            Config = new ConfigManager().GetConfig();
            Api = new(Config.HttpPostUrl);
            Request = new();
            RootPath = Environment.CurrentDirectory;
            CachePath = Path.Join(RootPath, "Cache", pluginName);
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
                CommandHandler.CommandEvent += async (cp,tokens) => {
                    if (commandInstance.Name.Equals(cp.Command) || commandInstance.CommandAlias.Contains(cp.Command))
                    {
                        CommandContext ctx = new(commandInstance.Name, this, cp);
                        ctx.Logger.Info("Command Triggered");
                        //While any node matches the token,control flow will execute this node and throw CommandFinishException to exit.
                        try
                        {
                            await commandInstance.Run(ctx, tokens);
                        }
                        catch (CommandAbortException)
                        {
                            //ignore
                        }
                        catch (InvalidArgumentException iae)
                        {
                            ctx.Logger.Error($"Invalid argument: {iae.ErrorToken}, require {iae.RequiredType}");
                        }
                        catch (TooLessArgumentException)
                        {
                            ctx.Logger.Error("To less arguments");
                        }
                        catch (PermissionDeniedException pde)
                        {
                            ctx.Logger.Error(
                                $"Not enough permission: {pde.CurrentPermission} at {pde.CurrentNode}, require {pde.RequiredPermission}");
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
