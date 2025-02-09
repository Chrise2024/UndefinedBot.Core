namespace UndefinedBot.Core.Command.CommandSource;

public sealed class UserCommandSource : BaseCommandSource
{
    public override string UserId { get; protected set; }
    public override string UserName { get; protected set; }
    public override string UserCard { get; protected set; }
    public override string GroupId { get; protected set; }
    public override CommandSourceAuthority Authority { get; protected set; }

    public override bool HasAuthorityLevel(CommandSourceAuthority authorityLevel)
    {
        return CurrentCommandAttrib.HasFlag(CommandAttribFlags.IgnoreRequirement) ||
               Authority >= authorityLevel;
    }

    public override bool IsFrom(CommandSourceType sourceType)
    {
        return sourceType == CommandSourceType.User;
    }

    private UserCommandSource(
        string userId,
        string groupId,
        string userName,
        CommandSourceAuthority authorityLevel,
        string userCard = ""
    )
    {
        UserId = userId;
        UserName = userName;
        UserCard = userCard;
        GroupId = groupId;
        Authority = authorityLevel;
    }

    public static UserCommandSource Friend(
        string userId,
        string userName,
        CommandSourceAuthority authorityLevel,
        string userCard = ""
    )
    {
        return new UserCommandSource(userId, "", userName, authorityLevel, userCard);
    }

    public static UserCommandSource Group(
        string userId,
        string groupId,
        string userName,
        CommandSourceAuthority authorityLevel,
        string userCard = ""
    )
    {
        return new UserCommandSource(userId, groupId, userName, authorityLevel, userCard);
    }
}