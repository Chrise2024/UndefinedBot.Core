using UndefinedBot.Core.Command;

namespace UndefinedBot.Core.Message;

public sealed class UserMessageSource : BaseMessageSource
{
    public override string UserId { get; protected set; }
    public override string UserName { get; protected set; }
    public override string UserCard { get; protected set; }
    public override string GroupId { get; protected set; }
    public override MessageSourceAuthority Authority { get; protected set; }

    public override bool HasAuthorityLevel(MessageSourceAuthority authorityLevel)
    {
        return CurrentCommandAttrib.HasFlag(CommandAttribFlags.IgnoreRequirement) ||
               Authority >= authorityLevel;
    }

    public override bool IsFrom(MessageSourceType sourceType)
    {
        return sourceType == MessageSourceType.User;
    }

    private UserMessageSource(
        string userId,
        string groupId,
        string userName,
        MessageSourceAuthority authorityLevel,
        string userCard = ""
    )
    {
        UserId = userId;
        UserName = userName;
        UserCard = userCard;
        GroupId = groupId;
        Authority = authorityLevel;
    }

    public static UserMessageSource Friend(
        string userId,
        string userName,
        MessageSourceAuthority authorityLevel,
        string userCard = ""
    )
    {
        return new UserMessageSource(userId, "", userName, authorityLevel, userCard);
    }

    public static UserMessageSource Group(
        string userId,
        string groupId,
        string userName,
        MessageSourceAuthority authorityLevel,
        string userCard = ""
    )
    {
        return new UserMessageSource(userId, groupId, userName, authorityLevel, userCard);
    }
}