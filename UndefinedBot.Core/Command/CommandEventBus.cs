using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandSource;

namespace UndefinedBot.Core.Command;

public delegate Task UniversalCommandEventHandler(CommandInvokeProperties invokeProperties,BaseCommandSource source);

/// <summary>
/// Plan to assign standalone bus for each adapter
/// </summary>
internal abstract class CommandEventBus
{
    private static event UniversalCommandEventHandler? UniversalCommandEvent;

    public static void InvokeCommandEvent(
        CommandInvokeProperties invokeProperties,
        BaseCommandSource source
        )
    {
        UniversalCommandEvent?.Invoke(invokeProperties, source);
    }

    public static void RegisterCommandEventHandler(UniversalCommandEventHandler handler)
    {
        UniversalCommandEvent += handler;
    }
}

/// <summary>
/// Contains the meta data of the command
/// </summary>
public class CommandInvokeProperties
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

    //Below properties will be filled by program
    public string AdapterId { get; private set; } = "";
    public string Platform { get; private set; } = "";
    public string Protocol { get; private set; } = "";
    public List<ParsedToken> Tokens { get; private set; } = [];
    public string GetEnvironmentInfo() => $"[{AdapterId}]{Platform}:{Protocol}";

    private CommandInvokeProperties(string command, long sourceId,int msgId, MessageSubType subType, long time)
    {
        Command = command;
        SourceId = sourceId;
        MsgId = msgId;
        SubType = subType;
        Time = time;
    }
    public static CommandInvokeProperties Group(string command, long sourceId, int msgId, long time)
    {
        return new CommandInvokeProperties(command, sourceId, msgId, MessageSubType.Group, time);
    }
    public static CommandInvokeProperties Friend(string command, long sourceId, int msgId, long time)
    {
        return new CommandInvokeProperties(command, sourceId, msgId, MessageSubType.Friend, time);
    }
    public static CommandInvokeProperties Guild(string command, long sourceId, int msgId, long time)
    {
        return new CommandInvokeProperties(command, sourceId, msgId, MessageSubType.Guild, time);
    }

    internal CommandInvokeProperties Implement(string adapterId, string platform, string protocol,List<ParsedToken> tokens)
    {
        AdapterId = adapterId;
        Platform = platform;
        Protocol = protocol;
        Tokens = tokens;
        return this;
    }
}
public enum MessageSubType
{
    Friend = 0,
    Group = 1,
    Guild = 2,
    Other = 3,
}


