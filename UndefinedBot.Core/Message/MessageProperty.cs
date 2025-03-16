using UndefinedBot.Core.Command;

namespace UndefinedBot.Core.Message;

public abstract class MessageProperty(string senderId, string sourceId, string msgId, MessageSubType subType, long timeStamp)
{
    /// <summary>
    /// Where the command from
    /// </summary>
    public readonly string SenderId = senderId;

    /// <summary>
    /// Where the command from
    /// </summary>
    public readonly string SourceId = sourceId;

    /// <summary>
    /// Command's message id
    /// </summary>
    public readonly string MsgId = msgId;

    /// <summary>
    /// Command's message sub type
    /// </summary>
    public readonly MessageSubType SubType = subType;

    /// <summary>
    /// Unix time samp,Command's message send time
    /// </summary>
    public readonly long TimeStamp = timeStamp;

    public string AdapterId { get; protected set; } = "";

    /// <summary>
    /// Platform of the adapter that submit the command
    /// </summary>
    public string Platform { get; protected set; } = "";

    /// <summary>
    /// Protocol of the adapter that submit the command
    /// </summary>
    public string Protocol { get; protected set; } = "";

    /// <summary>
    /// Check if the command is from group
    /// </summary>
    /// <returns></returns>
    public bool IsGroup()
    {
        return SubType == MessageSubType.Group;
    }

    /// <summary>
    /// Check if the command is from friend
    /// </summary>
    /// <returns></returns>
    public bool IsFriend()
    {
        return SubType == MessageSubType.Friend;
    }

    /// <summary>
    /// Check if the command is from guild
    /// </summary>
    /// <returns></returns>
    public bool IsGuild()
    {
        return SubType == MessageSubType.Guild;
    }

    /// <summary>
    /// Check if the command is valid
    /// </summary>
    /// <returns></returns>
    public bool IsValid()
    {
        return SubType != MessageSubType.Other;
    }

    /// <summary>
    /// Environment Information
    /// </summary>
    /// <returns></returns>
    public string GetEnvironmentInfo()
    {
        return $"[{AdapterId}]{Platform}:{Protocol}";
    }
}