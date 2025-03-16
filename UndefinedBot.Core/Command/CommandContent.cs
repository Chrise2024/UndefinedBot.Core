using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Message;

namespace UndefinedBot.Core.Command;



/// <summary>
/// Contains the meta data of the command
/// </summary>
/// <remarks>
/// Don't create instance directly.
/// </remarks>
public sealed class CommandContent : MessageProperty
{
    /// <summary>
    /// Command name
    /// </summary>
    public readonly string CalledCommandName;

    #region Constructor

    /// <summary>
    /// Id of the adapter that submit the command
    /// </summary>
    private CommandContent(string calledCommandName, string sourceId, string senderId, string msgId,
        MessageSubType subType, long timeStamp) : base(senderId, sourceId, msgId, subType, timeStamp)
    {
        CalledCommandName = calledCommandName;
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
    public static CommandContent Group(string command, string sourceId, string senderId, string msgId, long time)
    {
        return new CommandContent(command, sourceId, senderId, msgId, MessageSubType.Group, time);
    }

    /// <summary>
    /// Create Friend Message Event's Meta Data
    /// </summary>
    /// <param name="command">Command Name</param>
    /// <param name="sourceId">Where The Command From(Friend Uin)</param>
    /// <param name="msgId">Message Id of Message</param>
    /// <param name="time">Message Send Time</param>
    /// <returns></returns>
    public static CommandContent Friend(string command, string sourceId, string msgId, long time)
    {
        return new CommandContent(command, sourceId, sourceId, msgId, MessageSubType.Friend, time);
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
    public static CommandContent Guild(string command, string sourceId, string senderId, string msgId, long time)
    {
        return new CommandContent(command, sourceId, senderId, msgId, MessageSubType.Guild, time);
    }

    #endregion

    #region AutoImplement

    /// <summary>
    /// Command's original prefix
    /// </summary>
    public string CommandPrefix { get; private set; } = "";

    /// <summary>
    /// Command's tokens
    /// </summary>
    public ParsedToken[] Tokens { get; private set; } = [];

    internal CommandContent Implement(string adapterId, string platform, string protocol,
        ParsedToken[] tokens, string prefix)
    {
        AdapterId = adapterId;
        Platform = platform;
        Protocol = protocol;
        Tokens = tokens;
        CommandPrefix = prefix;
        return this;
    }

    #endregion
}