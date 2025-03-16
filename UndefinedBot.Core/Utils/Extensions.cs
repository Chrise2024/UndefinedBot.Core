using UndefinedBot.Core.Command.CommandUtils;
using UndefinedBot.Core.Message;
using UndefinedBot.Core.MessageProcessor;

namespace UndefinedBot.Core.Utils;

public static class ActionExtensions
{
    public static NodeActionWrapper TimeoutAfter(
        this Func<CommandContext, BaseMessageSource, CancellationToken, Task> nodeAction,
        TimeSpan timeout)
    {
        return new NodeActionWrapper(nodeAction, timeout);
    }

    public static MsgProcessorWrapper TimeoutAfter(
        this Func<ProcessorContext,BaseMessageSource,CancellationToken,Task> processor,
        TimeSpan timeout)
    {
        return new MsgProcessorWrapper(processor, timeout);
    }
}

public readonly struct NodeActionWrapper(
    Func<CommandContext, BaseMessageSource, CancellationToken, Task> nodeAction,
    TimeSpan timeout)
{
    public async Task Invoke(CommandContext ctx, BaseMessageSource source)
    {
        CancellationTokenSource cts = new(timeout);
        try
        {
            await nodeAction.Invoke(ctx, source, cts.Token).WaitAsync(timeout, cts.Token);
        }
        catch (TimeoutException)
        {
            await cts.CancelAsync();
        }
        finally
        {
            cts.Dispose();
        }
    }
}

public readonly struct MsgProcessorWrapper(
    Func<ProcessorContext,BaseMessageSource,CancellationToken,Task> processor,
    TimeSpan timeout)
{
    public async Task Invoke(ProcessorContext ctx, BaseMessageSource source)
    {
        CancellationTokenSource cts = new(timeout);
        try
        {
            await processor.Invoke(ctx, source, cts.Token).WaitAsync(timeout, cts.Token);
        }
        catch (TimeoutException)
        {
            await cts.CancelAsync();
        }
        finally
        {
            cts.Dispose();
        }
    }
}