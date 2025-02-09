using UndefinedBot.Core.Plugin.BasicMessage;

namespace UndefinedBot.Core.Adapter.ActionParam;

public sealed class SendPrivateMsgParam : IDefaultActionParam
{
    public required IMessageNode[] MessageChain;
}