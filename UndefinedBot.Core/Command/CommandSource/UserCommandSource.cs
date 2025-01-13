namespace UndefinedBot.Core.Command.CommandSource;

public sealed class UserCommandSource : BaseCommandSource
{
    public override long UserId { get; protected set; }
    public override string UserName { get; protected set; }
    public override string UserCard { get; protected set; }
    public override long GroupId { get; protected set; }
    public override CommandSourceAuthority Authority { get; protected set; }

    public override bool HasAuthorityLevel(CommandSourceAuthority authorityLevel)
    {
        return CurrentCommandAttrib.HasFlag(CommandAttribFlags.IgnoreAuthority) ||
               Authority >= authorityLevel;
    }

    public override bool IsFrom(CommandSourceType sourceType)
    {
        return sourceType == CommandSourceType.User;
    }

    private UserCommandSource(
        long userId,
        long groupId,
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
        long userId,
        string userName,
        CommandSourceAuthority authorityLevel,
        string userCard = ""
    )
    {
        return new UserCommandSource(userId, 0, userName, authorityLevel, userCard);
    }

    public static UserCommandSource Group(
        long userId,
        long groupId,
        string userName,
        CommandSourceAuthority authorityLevel,
        string userCard = ""
    )
    {
        return new UserCommandSource(userId, groupId, userName, authorityLevel, userCard);
    }
}