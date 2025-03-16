using UndefinedBot.Core.Command;
using UndefinedBot.Core.Message;

namespace UndefinedBot.Core.MessageProcessor;

public class MessageContent : MessageProperty
{
    public readonly string MessageString;

    #region Constructor
    
    private MessageContent(string messageString,string sourceId, string senderId, string msgId,
        MessageSubType subType, long timeStamp) : base(senderId, sourceId, msgId, subType, timeStamp)
    {
        MessageString = messageString;
    }
    
    /// <summary>
    /// Create Group Message Event's Meta Data
    /// </summary>
    /// <param name="messageString">Raw Message</param>
    /// <param name="sourceId">Where The Command From(Group Id)</param>
    /// <param name="senderId">Who called this command(User Id)</param>
    /// <param name="msgId">Message Id of Message</param>
    /// <param name="time">Message Send Time</param>
    /// <returns></returns>
    public static MessageContent Group(string messageString, string sourceId, string senderId, string msgId, long time)
    {
        return new MessageContent(messageString, sourceId, senderId, msgId, MessageSubType.Group, time);
    }

    /// <summary>
    /// Create Friend Message Event's Meta Data
    /// </summary>
    /// <param name="messageString">Raw Message</param>
    /// <param name="sourceId">Where The Command From(Friend Uin)</param>
    /// <param name="msgId">Message Id of Message</param>
    /// <param name="time">Message Send Time</param>
    /// <returns></returns>
    public static MessageContent Friend(string messageString, string sourceId, string msgId, long time)
    {
        return new MessageContent(messageString, sourceId, sourceId, msgId, MessageSubType.Friend, time);
    }

    /// <summary>
    /// Create Guild Message Event's Meta Data
    /// </summary>
    /// <param name="messageString">Raw Message</param>
    /// <param name="sourceId">Where The Command From(Guild Id)</param>
    /// <param name="senderId">Who called this command(User Id)</param>
    /// <param name="msgId">Message Id of Message</param>
    /// <param name="time">Message Send Time</param>
    /// <returns></returns>
    public static MessageContent Guild(string messageString, string sourceId, string senderId, string msgId, long time)
    {
        return new MessageContent(messageString, sourceId, senderId, msgId, MessageSubType.Guild, time);
    }

    #endregion

    #region AutoImplement
    
    internal MessageContent Implement(string adapterId, string platform, string protocol)
    {
        AdapterId = adapterId;
        Platform = platform;
        Protocol = protocol;
        return this;
    }

    #endregion
}