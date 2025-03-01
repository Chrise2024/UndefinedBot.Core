using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Command.CommandUtils;

namespace UndefinedBot.Core.Utils;

public static class ActionExtensions
{
    public static NodeActionWrapper TimeoutAfter(this Func<CommandContext, BaseCommandSource, CancellationToken, Task> nodeAction,
        TimeSpan timeout) => new(nodeAction, timeout);
}

public readonly struct NodeActionWrapper(Func<CommandContext, BaseCommandSource, CancellationToken, Task> nodeAction,TimeSpan timeout)
{
    public async Task Invoke(CommandContext ctx, BaseCommandSource source)
    {
        CancellationTokenSource cts = new(timeout);
        try
        {
            await nodeAction.Invoke(ctx, source,cts.Token).WaitAsync(timeout,cts.Token);
        }
        catch (OperationCanceledException)
        {
            await cts.CancelAsync();
        }
        finally
        {
            cts.Dispose();
        }
    }
}