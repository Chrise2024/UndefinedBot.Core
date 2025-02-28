using System.Text.Json;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.CommandException;
using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Command.CommandUtils;
using UndefinedBot.Core.Utils.Logging;

namespace UndefinedBot.Core.Service;

internal sealed class CommandService : IDisposable
{
    private readonly Dictionary<string, List<CommandInstance>> _commandIndex = [];
    private List<CommandInstance> _commands = [];
    private readonly HelpCommand _helpCommand;
    private readonly InternalLogger _logger = new(["CommandService"]);
    private Task? CommandLoopTask { get; set; }
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    public CommandService()
    {
        _helpCommand = new(_commandIndex);
    }

    public void LoadCommand(List<CommandInstance> commands)
    {
        _commands = commands;
        IndexCommand();
    }
    
    public void Unload()
    {
        foreach (var ci in _commands)
        {
            ci.Dispose();
        }
        _commands.Clear();
        _commandIndex.Clear();
    }
    
    public void StartCommandLoop()
    {
        _logger.Info("Logging loop started");
        CommandLoopTask = CommandEventLoop(_cancellationTokenSource.Token);
    }

    public void StopCommandLoop()
    {
        _logger.Info("Logging loop stopped");
        _cancellationTokenSource.Cancel();
    }
    

    private void IndexCommand()
    {
        foreach (var ci in _commands)
        {
            foreach (string tai in ci.TargetAdapterId)
            {
                if (_commandIndex.TryAdd(tai, [ci]))
                {
                    continue;
                }
                _commandIndex[tai].Add(ci);
            }
        }
    }

    private async Task CommandEventLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            CommandEventWrapper commandEvent = await CommandEventBus.ReadCommandEventAsync(token);
            if (commandEvent.BasicInformation.CalledCommandName.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                _helpCommand.InvokeHelpCommandAsync(commandEvent.BasicInformation, commandEvent.Source);
                continue;
            }

            HandleCommandAsync(commandEvent);
        }
    }
    private async void HandleCommandAsync(CommandEventWrapper commandEvent)
    {
        if (!_commandIndex.TryGetValue(commandEvent.BasicInformation.AdapterId, out List<CommandInstance>? refCollection))
        {
            _logger.Warn($"No command bind to adapter: {commandEvent.BasicInformation.AdapterId}");
            return;
        }
        
        CommandInstance? targetCommand = refCollection.Find(t => t.IsTargetCommand(commandEvent.BasicInformation,commandEvent.Source));
        if (targetCommand is null)
        {
            _logger.Warn($"No such command: {commandEvent.BasicInformation.CalledCommandName}");
            return;
        }
        
        if (targetCommand.IsReachRateLimit(commandEvent.BasicInformation))
        {
            _logger.Warn($"Command: {commandEvent.BasicInformation.CalledCommandName} reached rate limit.");
            return;
        }
        
        CommandContext ctx = new(targetCommand, commandEvent.BasicInformation);
        ctx.Logger.Info("Command triggered");
        
        try
        {
            ICommandResult result = await targetCommand.RunAsync(ctx, commandEvent.Source, commandEvent.BasicInformation.Tokens);
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
        _cancellationTokenSource.Dispose();
        _commands.Clear();
        _commandIndex.Clear();
        _logger.Dispose();
    }
}