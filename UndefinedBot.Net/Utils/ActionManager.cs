using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Adapter.ActionParam;

namespace UndefinedBot.Net.Utils;

public sealed class ActionManager(IAdapterInstance parentAdapter) : IActionManager
{
    public async Task<byte[]?> InvokeAction(ActionType action, string? target = null, IActionParam? parameter = null)
    {
        return await parentAdapter.HandleActionAsync(action, target, parameter);
    }
}