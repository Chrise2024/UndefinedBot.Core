using UndefinedBot.Core.Command;
using UndefinedBot.Core.Message;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.MessageProcessor;

public sealed class MessageProcessorInstance
{
    internal string Name { get; }
    internal string PluginId { get; }
    internal string[] TargetAdapterId { get; }
    private ILogger Logger { get; }

    private MsgProcessorAttribFlags ProcessorAttrib { get; set; }
    internal CacheManager Cache { get; }

    private Func<ProcessorContext, BaseMessageSource, bool>? FilterCondition { get; set; }
    private Func<ProcessorContext, BaseMessageSource, CancellationToken, Task>? Processor { get; set; }
    private bool InvertCondition { get; set; }

    internal MessageProcessorInstance(string filterName, string pluginId, string[] target, ILogger parentLogger)
    {
        Name = filterName;
        PluginId = pluginId;
        TargetAdapterId = target;
        Logger = parentLogger.Extend(["MsgFilter", filterName]);
        Cache = new CacheManager(pluginId, Logger);
    }

    internal ILogger AcquireLogger()
    {
        return Logger;
    }

    /// <summary>
    /// Set filter
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="invert">If false,msg that make FilterCondition return true will be processed.</param>
    /// <returns></returns>
    public MessageProcessorInstance Require(Func<ProcessorContext, BaseMessageSource, bool> predicate,
        bool invert = false)
    {
        FilterCondition = predicate;
        InvertCondition = invert;
        return this;
    }

    public MessageProcessorInstance Attrib(MsgProcessorAttribFlags attrib)
    {
        ProcessorAttrib = attrib;
        return this;
    }

    public void Execute(Func<ProcessorContext, BaseMessageSource, CancellationToken, Task> processor)
    {
        Processor = processor;
    }

    internal async Task RunAsync(ProcessorContext ctx, BaseMessageSource source, Action callback)
    {
        if (Processor is null)
        {
            return;
        }

        if (!ShouldProcess(ctx, source))
        {
            return;
        }

        await Processor.TimeoutAfter(TimeSpan.FromSeconds(20)).Invoke(ctx, source);
        callback.Invoke();
    }

    private bool ShouldProcess(ProcessorContext ctx, BaseMessageSource source)
    {
        switch (ctx.Content.SubType)
        {
            case MessageSubType.Friend when !ProcessorAttrib.HasFlag(MsgProcessorAttribFlags.ActiveInFriend):
            case MessageSubType.Group when !ProcessorAttrib.HasFlag(MsgProcessorAttribFlags.ActiveInGroup):
            case MessageSubType.Guild when !ProcessorAttrib.HasFlag(MsgProcessorAttribFlags.ActiveInGuild):
                return false;
        }

        if (!ProcessorAttrib.HasFlag(MsgProcessorAttribFlags.IgnoreAuthority) &&
            source.HasAuthorityLevel(MessageSourceAuthority.Admin))
        {
            return false;
        }

        return ProcessorAttrib.HasFlag(MsgProcessorAttribFlags.IgnoreCondition) ||
               FilterCondition is null ||
               InvertCondition ^ FilterCondition.Invoke(ctx, source);
        //Invert false,Process if FilterCondition is true
        //XOR
    }
}

[Flags]
public enum MsgProcessorAttribFlags
{
    /// <summary>
    /// The command can be triggered in friend chat
    /// </summary>
    ActiveInFriend = 0b_0000_0000_0000_0001,

    /// <summary>
    /// The command can be triggered in group chat
    /// </summary>
    ActiveInGroup = 0b_0000_0000_0000_0010,

    /// <summary>
    /// The command can be triggered in guild chat
    /// </summary>
    ActiveInGuild = 0b_0000_0000_0000_0100,

    /// <summary>
    /// The command can be triggered without authority check
    /// </summary>
    IgnoreCondition = 0b_0000_0000_0000_1000,

    /// <summary>
    /// The command can be triggered with ignoring case
    /// </summary>
    IgnoreCase = 0b_0000_0000_0001_0000,

    /// <summary>
    /// The command can be triggered with ignoring case
    /// </summary>
    IgnoreAuthority = 0b_0000_0000_0010_0000,
}