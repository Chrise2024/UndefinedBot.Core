using UndefinedBot.Core.Command;

namespace UndefinedBot.Core.Message;

public abstract class BaseMessageSource
{
    public virtual string UserId { get; protected set; } = "";
    public virtual string UserName { get; protected set; } = "";
    public virtual string UserCard { get; protected set; } = "";
    public virtual string GroupId { get; protected set; } = "";
    public virtual MessageSourceAuthority Authority { get; protected set; } = MessageSourceAuthority.InvalidSource;
    public abstract bool HasAuthorityLevel(MessageSourceAuthority authorityLevel);
    public abstract bool IsFrom(MessageSourceType sourceType);
    internal CommandAttribFlags CurrentCommandAttrib { get; private set; } = CommandInstance.DefaultCommandAttrib;

    internal void SetCurrentCommandAttrib(CommandAttribFlags attr)
    {
        CurrentCommandAttrib = attr;
    }
}

public enum MessageSourceType
{
    User = 0,
    Console = 1,
    Redirect = 2
}

public enum MessageSourceAuthority
{
    InvalidSource = 0,
    User = 1,
    PowerUser = 2,
    Admin = 3,
    Operator = 4,
    Console = 5
}