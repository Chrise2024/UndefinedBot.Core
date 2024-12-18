using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandSource;

namespace UndefinedBot.Core.Command;

public delegate Task UniversalCommandEventHandler(CommandInvokeProperties invokeProperties,BaseCommandSource source);

internal abstract class CommandEventBus
{
    private static event UniversalCommandEventHandler? UniversalCommandEvent;

    internal static void InvokeCommandEvent(
        CommandInvokeProperties invokeProperties,
        BaseCommandSource source
        )
    {
        UniversalCommandEvent?.Invoke(invokeProperties, source);
    }

    internal static void RegisterCommandEventHandler(UniversalCommandEventHandler handler)
    {
        UniversalCommandEvent += handler;
    }
}

public class PrimeInvokeProperties
{
    public string Command { get; }
    public long SourceId { get; }
    public int MsgId { get; }
    public MessageSubType SubType { get; }
    public long Time { get; }

    public bool IsGroup() => SubType == MessageSubType.Group;

    public bool IsFriend() => SubType == MessageSubType.Friend;

    public bool IsGuild() => SubType == MessageSubType.Guild;

    public bool IsValid() => SubType != MessageSubType.Other;
    private PrimeInvokeProperties(string command, long sourceId,int msgId, MessageSubType subType, long time)
    {
        Command = command;
        SourceId = sourceId;
        MsgId = msgId;
        SubType = subType;
        Time = time;
    }
    protected PrimeInvokeProperties(PrimeInvokeProperties parentInstance)
    {
        Command = parentInstance.Command;
        SourceId = parentInstance.SourceId;
        MsgId = parentInstance.MsgId;
        SubType = parentInstance.SubType;
        Time = parentInstance.Time;
    }
    public static PrimeInvokeProperties Group(string command, long sourceId, int msgId, long time)
    {
        return new PrimeInvokeProperties(command, sourceId, msgId, MessageSubType.Group, time);
    }
    public static PrimeInvokeProperties Friend(string command, long sourceId, int msgId, long time)
    {
        return new PrimeInvokeProperties(command, sourceId, msgId, MessageSubType.Friend, time);
    }
    public static PrimeInvokeProperties Guild(string command, long sourceId, int msgId, long time)
    {
        return new PrimeInvokeProperties(command, sourceId, msgId, MessageSubType.Guild, time);
    }

    internal CommandInvokeProperties ImplementInvokeProperties(string platform, string protocol, List<ParsedToken> tokens)
    {
        return new CommandInvokeProperties(this, platform, protocol, tokens);
    }
}

public class CommandInvokeProperties : PrimeInvokeProperties
{
    public string Platform { get; }
    public string Protocol { get; }
    public List<ParsedToken> Tokens { get; }

    public string GetEnvironmentInfo() => $"{Platform}:{Protocol}";
    internal CommandInvokeProperties(PrimeInvokeProperties parent, string platform, string protocol, List<ParsedToken> tokens) : base(parent)
    {
        Platform = platform;
        Protocol = protocol;
        Tokens = tokens;
    }
}


