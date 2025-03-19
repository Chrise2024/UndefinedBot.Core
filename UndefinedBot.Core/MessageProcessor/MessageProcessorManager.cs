using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Message;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.MessageProcessor;

internal sealed class MessageProcessorManager
{
    private readonly List<MessageProcessorInstance> _messageProcessorInstances;

    private readonly ILogger _logger;

    private readonly IAdapterInstance _parentAdapter;

    public MessageProcessorManager(IAdapterInstance parentAdapter, List<MessageProcessorInstance> messageProcessorInstances)
    {
        _logger = parentAdapter.AcquireLogger().Extend(nameof(MessageProcessorManager));
        _parentAdapter = parentAdapter;
        _messageProcessorInstances = messageProcessorInstances;
    }

    public async Task ProcessMessageAsync(MessageContent content, BaseMessageSource source)
    {
        await Task.WhenAny(_messageProcessorInstances.Select(i =>
        {
            ProcessorContext ctx = new(i, content, new ActionManager(_parentAdapter));
            ctx.Dispose();
            return i.RunAsync(ctx, source,() => ctx.Dispose());
        }));
    }
}