using UndefinedBot.Core.Command.Arguments;

namespace UndefinedBot.Core.Command;

/// <summary>
/// Contains the meta data of the command
/// </summary>
/// <example>
/// <code lang="CSharp">CommandInvokeProperties properties = CommandInvokeProperties.Group("homo", 114514, 114514, 114514);</code>
/// </example>
/// <remarks>
/// Don't create instance directly.
/// </remarks>
public sealed class CommandInformation : IDisposable
{
    /// <summary>
    /// Command name
    /// </summary>
    public readonly string CalledCommandName;

    /// <summary>
    /// Where the command from
    /// </summary>
    public readonly string SenderId;

    /// <summary>
    /// Where the command from
    /// </summary>
    public readonly string SourceId;

    /// <summary>
    /// Command's message id
    /// </summary>
    public readonly string MsgId;

    /// <summary>
    /// Command's message sub type
    /// </summary>
    public readonly MessageSubType SubType;

    /// <summary>
    /// Unix time samp,Command's message send time
    /// </summary>
    public readonly long TimeStamp;
    /// <summary>
    /// Check if the command is from group
    /// </summary>
    /// <returns></returns>
    public bool IsGroup() => SubType == MessageSubType.Group;
    /// <summary>
    /// Check if the command is from friend
    /// </summary>
    /// <returns></returns>
    public bool IsFriend() => SubType == MessageSubType.Friend;
    /// <summary>
    /// Check if the command is from guild
    /// </summary>
    /// <returns></returns>
    public bool IsGuild() => SubType == MessageSubType.Guild;
    /// <summary>
    /// Check if the command is valid
    /// </summary>
    /// <returns></returns>
    public bool IsValid() => SubType != MessageSubType.Other;
    //Below properties will be filled by program
    
    /// <summary>
    /// Id of the adapter that submit the command
    /// </summary>
    public string AdapterId { get; private set; } = "";
    /// <summary>
    /// Platform of the adapter that submit the command
    /// </summary>
    public string Platform { get; private set; } = "";
    /// <summary>
    /// Protocol of the adapter that submit the command
    /// </summary>
    public string Protocol { get; private set; } = "";
    /// <summary>
    /// Command's original prefix
    /// </summary>
    public string CommandPrefix { get; private set; } = "";
    /// <summary>
    /// Command's tokens
    /// </summary>
    public ParsedToken[] Tokens { get; private set; } = [];
    /// <summary>
    /// Environment Information
    /// </summary>
    /// <returns></returns>
    public string GetEnvironmentInfo() => $"[{AdapterId}]{Platform}:{Protocol}";

    private CommandInformation(string calledCommandName, string sourceId,string senderId, string msgId, MessageSubType subType, long timeStamp)
    {
        CalledCommandName = calledCommandName;
        SourceId = sourceId;
        SenderId = senderId;
        MsgId = msgId;
        SubType = subType;
        TimeStamp = timeStamp;
    }
    /// <summary>
    /// Create Group Message Event's Meta Data
    /// </summary>
    /// <param name="command">Command Name</param>
    /// <param name="sourceId">Where The Command From(Group Id)</param>
    /// <param name="senderId">Who called this command(User Id)</param>
    /// <param name="msgId">Message Id of Message</param>
    /// <param name="time">Message Send Time</param>
    /// <returns></returns>
    public static CommandInformation Group(string command, string sourceId,string senderId, string msgId, long time)
    {
        return new CommandInformation(command, sourceId,senderId, msgId, MessageSubType.Group, time);
    }
    /// <summary>
    /// Create Friend Message Event's Meta Data
    /// </summary>
    /// <param name="command">Command Name</param>
    /// <param name="sourceId">Where The Command From(Friend Uin)</param>
    /// <param name="msgId">Message Id of Message</param>
    /// <param name="time">Message Send Time</param>
    /// <returns></returns>
    public static CommandInformation Friend(string command, string sourceId, string msgId, long time)
    {
        return new CommandInformation(command, sourceId,sourceId, msgId, MessageSubType.Friend, time);
    }
    /// <summary>
    /// Create Guild Message Event's Meta Data
    /// </summary>
    /// <param name="command">Command Name</param>
    /// <param name="sourceId">Where The Command From(Guild Id)</param>
    /// <param name="senderId">Who called this command(User Id)</param>
    /// <param name="msgId">Message Id of Message</param>
    /// <param name="time">Message Send Time</param>
    /// <returns></returns>
    public static CommandInformation Guild(string command, string sourceId,string senderId, string msgId, long time)
    {
        return new CommandInformation(command, sourceId,senderId, msgId, MessageSubType.Guild, time);
    }
    internal CommandInformation Implement(string adapterId, string platform, string protocol,
        ParsedToken[] tokens, string prefix)
    {
        AdapterId = adapterId;
        Platform = platform;
        Protocol = protocol;
        Tokens = tokens;
        CommandPrefix = prefix;
        return this;
    }
    public void Dispose()
    {
        Tokens = [];
    }
}