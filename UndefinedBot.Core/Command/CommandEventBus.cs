using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandSource;

namespace UndefinedBot.Core.Command;

/// <summary>
/// Contains the meta data of the command
/// </summary>
public sealed class CommandInvokeProperties
{
    public string Command { get; }
    public long SourceId { get; }
    public int MsgId { get; }
    public MessageSubType SubType { get; }
    public long TimeStamp { get; }
    public bool IsGroup() => SubType == MessageSubType.Group;
    public bool IsFriend() => SubType == MessageSubType.Friend;
    public bool IsGuild() => SubType == MessageSubType.Guild;
    public bool IsValid() => SubType != MessageSubType.Other;
    //Below properties will be filled by program
    public string AdapterId { get; private set; } = "";
    public string Platform { get; private set; } = "";
    public string Protocol { get; private set; } = "";
    public string CommandPrefix { get; private set; } = "";
    public List<ParsedToken> Tokens { get; private set; } = [];
    public string GetEnvironmentInfo() => $"[{AdapterId}]{Platform}:{Protocol}";

    private CommandInvokeProperties(string command, long sourceId, int msgId, MessageSubType subType, long timeStamp)
    {
        Command = command;
        SourceId = sourceId;
        MsgId = msgId;
        SubType = subType;
        TimeStamp = timeStamp;
    }
    /// <summary>
    /// Create Group Message Event's Meta Data
    /// </summary>
    /// <param name="command">Command Name</param>
    /// <param name="sourceId">Where The Command From(Group Id)</param>
    /// <param name="msgId">Message Id of Message</param>
    /// <param name="time">Message Send Time</param>
    /// <returns></returns>
    public static CommandInvokeProperties Group(string command, long sourceId, int msgId, long time)
    {
        return new CommandInvokeProperties(command, sourceId, msgId, MessageSubType.Group, time);
    }
    /// <summary>
    /// Create Friend Message Event's Meta Data
    /// </summary>
    /// <param name="command">Command Name</param>
    /// <param name="sourceId">Where The Command From(Friend Uin)</param>
    /// <param name="msgId">Message Id of Message</param>
    /// <param name="time">Message Send Time</param>
    /// <returns></returns>
    public static CommandInvokeProperties Friend(string command, long sourceId, int msgId, long time)
    {
        return new CommandInvokeProperties(command, sourceId, msgId, MessageSubType.Friend, time);
    }
    /// <summary>
    /// Create Guild Message Event's Meta Data
    /// </summary>
    /// <param name="command">Command Name</param>
    /// <param name="sourceId">Where The Command From(Guild Id)</param>
    /// <param name="msgId">Message Id of Message</param>
    /// <param name="time">Message Send Time</param>
    /// <returns></returns>
    public static CommandInvokeProperties Guild(string command, long sourceId, int msgId, long time)
    {
        return new CommandInvokeProperties(command, sourceId, msgId, MessageSubType.Guild, time);
    }
    internal CommandInvokeProperties Implement(string adapterId, string platform, string protocol,
        List<ParsedToken> tokens, string prefix)
    {
        AdapterId = adapterId;
        Platform = platform;
        Protocol = protocol;
        Tokens = tokens;
        CommandPrefix = prefix;
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