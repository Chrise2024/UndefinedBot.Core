using UndefinedBot.Core.Adapter.ActionParam;

namespace UndefinedBot.Core.Adapter;

public sealed class ActionManager(IAdapterInstance parentAdapter)
{
    public async Task<byte[]?> InvokeAction(ActionType action, string? target = null, IActionParam? parameter = null)
    {
        return await parentAdapter.HandleActionAsync(action, target, parameter);
    }
}

public enum ActionType
{
    SendPrivateMsg = 0,
    SendGroupMsg = 1,
    RecallMessage = 2,
    GetMessage = 3,
    GetGroupMemberInfo = 4,
    GetGroupMemberList = 5,
    GroupMute = 6,
    GroupKick = 7,
    Custom = 8
}