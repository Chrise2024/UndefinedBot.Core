using System.Text.Json.Nodes;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Adapter;

public abstract class BaseAdapter(AdapterConfigData adapterConfig)
{
    protected abstract string Name { get; }
    protected abstract string Platform { get; }
    protected abstract string Protocol { get; }
    protected AdapterLogger Logger => new(Name);
    protected AdapterConfigData AdapterConfig => adapterConfig;
    protected void SubmitCommandEvent(
        PrimeInvokeProperties invokeProperties,
        BaseCommandSource source,
        List<ParsedToken> tokens
        )
    {
        CommandEventBus.InvokeCommandEvent(invokeProperties.ImplementInvokeProperties(Platform, Protocol, tokens), source);
    }
    public abstract void HandleAdapterAction(string action, object paras);
    public abstract void HandleAdapterAction(DefaultActionType action, object paras);
}

[Serializable] public class AdapterConfigData
{
    public string EntryFile { get; set; } = "";
    public string Description { get; set; } = "";
    public string CommandPrefix { get; set; } = "!";
    public List<long> GroupIds { get; set; } = [];
    public JsonNode OriginalConfig { get; set; } = JsonNode.Parse("{}")!;
}

public enum DefaultActionType
{
    SendPrivateMsg = 0,
    SendGroupMsg = 1,
    RecallMessage = 2,
    GetMessage = 3,
    GetGroupMemberInfo = 4,
    GetGroupMemberList = 5,
    GroupMute = 6,
    GroupKick = 7,
}
