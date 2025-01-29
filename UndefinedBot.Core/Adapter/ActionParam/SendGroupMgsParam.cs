using UndefinedBot.Core.Plugin.BasicMessage;

namespace UndefinedBot.Core.Adapter.ActionParam;

public sealed class SendGroupMgsParam : IActionParam
{
    public required IMessageNode[] MessageChain;
}