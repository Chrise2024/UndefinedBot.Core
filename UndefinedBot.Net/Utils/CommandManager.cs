﻿using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandException;
using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Command.CommandUtils;
using UndefinedBot.Core.Plugin;
using UndefinedBot.Core.Utils;
using UndefinedBot.Net.Utils.Logging;

namespace UndefinedBot.Net.Utils;

internal sealed class CommandManager : ICommandManager
{
    private readonly List<CommandInstance> _commandInstances;

    private readonly HelpCommand _helpCommand;

    private readonly ILogger _logger;

    private readonly IAdapterInstance _parentAdapter;

    public CommandManager(IServiceProvider provider, IAdapterInstance parentAdapter)
    {
        _commandInstances = provider.GetRequiredService<PluginLoadService>().AcquireCommandInstance(parentAdapter.Id);
        _helpCommand = new HelpCommand(_commandInstances, new ActionManager(parentAdapter),
            new CommandLogger(provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CommandLogger>>(),
                "Core", "Help"));
        _logger = parentAdapter.AcquireLogger().Extend("CommandManager");
        _parentAdapter = parentAdapter;
    }

    public async void InvokeCommandAsync(
        CommandInformation information,
        BaseCommandSource source,
        ParsedToken[] tokens
    )
    {
        if (information.CalledCommandName.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            _helpCommand.InvokeHelpCommandAsync(information, source);
            return;
        }

        CommandInstance? targetCommand = _commandInstances.Find(t => t.IsTargetCommand(information, source));
        if (targetCommand is null)
        {
            _logger.Warn($"No such command: {information.CalledCommandName}");
            return;
        }

        if (targetCommand.IsReachRateLimit(information))
        {
            _logger.Warn($"Command: {information.CalledCommandName} reached rate limit.");
            return;
        }

        CommandContext ctx = new(targetCommand, information, new ActionManager(_parentAdapter));
        ctx.Logger.Info("Command triggered");

        try
        {
            ICommandResult result = await targetCommand.RunAsync(ctx, source, information.Tokens);
            switch (result)
            {
                case CommandSuccess:
                    //ignore
                    break;
                case InvalidArgumentCommandResult iae:
                    ctx.Logger.Error(
                        $"Invalid argument: {iae.ErrorToken}, require {JsonSerializer.Serialize(iae.RequiredType)}");
                    break;
                case TooLessArgument tae:
                    ctx.Logger.Error(
                        $"To less arguments, require {JsonSerializer.Serialize(tae.RequiredType)}");
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
            ctx.Logger.Error(ex, "Command Failed");
        }

        ctx.Dispose();
        targetCommand.Cache.UpdateCache();
    }

    public void Dispose()
    {
        _helpCommand.Dispose();
        _commandInstances.Clear();
    }
}