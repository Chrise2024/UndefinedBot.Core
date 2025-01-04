namespace UndefinedBot.Core.Command.CommandResult;

internal sealed class PermissionDenied(string currentNode, string currentPermission, string requiredPermission)
    : ICommandResult
{
    public ExecuteStatus Status => ExecuteStatus.PermissionDenied;
    public string CurrentNode => currentNode;
    public string CurrentPermission => currentPermission;
    public string RequiredPermission => requiredPermission;
}