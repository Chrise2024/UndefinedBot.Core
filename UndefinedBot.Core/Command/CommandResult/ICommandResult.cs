
namespace UndefinedBot.Core.Command.CommandResult;

public enum ExecuteStatus
{
    Success = 0,
    Fail = 1,
    PermissionDenied = 2,
    InvalidArgument = 3,
    NullArgument = 4,
    InvalidSyntax = 5,
}
public interface ICommandResult
{
    //public string? ErrorToken { get; }
    //public string? ErrorInfo { get; }
    public ExecuteStatus Status { get; }
}