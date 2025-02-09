using UndefinedBot.Core.Plugin.BasicMessage;

namespace UndefinedBot.Core.Adapter.ActionParam;

public sealed class SendGroupMgsParam : IDefaultActionParam
{
    public required IMessageNode[] MessageChain;
}