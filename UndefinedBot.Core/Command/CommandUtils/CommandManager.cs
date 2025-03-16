using System.Text.Json;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Command.CommandException;
using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Message;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Command.CommandUtils;

internal sealed class CommandManager
{
    private readonly List<CommandInstance> _commandInstances;

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
            new ActionManager(parentAdapter),
            _logger
        );
    }

    public async void InvokeCommandAsync(
        CommandContent content,
        BaseMessageSource source
    )
    {
        if (content.CalledCommandName.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            _helpCommand.InvokeHelpCommandAsync(content, source);
            return;
        }

        CommandInstance? targetCommand = _commandInstances.Find(t => t.IsTargetCommand(content, source));
        if (targetCommand is null)
        {
            _logger.Warn($"No such command: {content.CalledCommandName}");
            return;
        }

        if (targetCommand.IsReachRateLimit(content))
        {
            _logger.Warn($"Command: {content.CalledCommandName} reached rate limit.");
            return;
        }

        CommandContext ctx = new(targetCommand, content, new ActionManager(_parentAdapter));
        ctx.Logger.Info("Command triggered");

        try
        {
            ICommandResult result = await targetCommand.RunAsync(ctx, source, content.Tokens);
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