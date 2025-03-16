using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.MessageProcessor;

public sealed class ProcessorContext : IDisposable
{
    public readonly string PluginName;
    public readonly string RootPath = Environment.CurrentDirectory;
    public readonly MessageContent Content;
    public readonly ILogger Logger;
    public readonly CacheManager Cache;
    public readonly HttpRequest Request;
    public readonly ActionManager Action;

    public ProcessorContext(MessageProcessorInstance processorInstance, MessageContent content, ActionManager action)
    {
        PluginName = processorInstance.PluginId;
        Content = content;
        Logger = processorInstance.AcquireLogger();
        Cache = processorInstance.Cache;
        Request = new HttpRequest(Logger);
        Action = action;
    }
    public void Dispose()
    {
        Request.Dispose();
    }
}