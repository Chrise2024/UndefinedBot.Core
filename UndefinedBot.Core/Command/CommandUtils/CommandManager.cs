using System.Text.Json;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandException;
using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Plugin;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Command.CommandUtils;

internal sealed class CommandManager
{
    private readonly List<CommandInstance> _commandInstances = [];

    private readonly HelpCommand _helpCommand;

    private readonly ILogger _logger;

    private readonly IAdapterInstance _parentAdapter;

    public CommandManager(IAdapterInstance parentAdapter, List<CommandInstance> commandInstances)
    {
        _logger = parentAdapter.AcquireLogger().Extend(nameof(CommandManager));
        _parentAdapter = parentAdapter;
        _commandInstances = commandInstances;
        _helpCommand = new HelpCommand(
            _commandInstances,
            new ActionManager(parentAdapter)
        );
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
            ctx.Logger.Error("Command execute aborted");
        }
        catch (CommandSyntaxException cse)
        {
            ctx.Logger.Error($"Node {cse.CurrentNode} not implemented");
        }
        catch (Exception ex)
        {
            ctx.Logger.Error(ex, "Command failed");
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