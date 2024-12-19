using System.Text.Json.Nodes;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Adapter;

public abstract class BaseAdapter(AdapterConfigData adapterConfig)
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Platform { get; }
    public abstract string Protocol { get; }
    protected AdapterLogger Logger => new(Name);
    protected AdapterConfigData AdapterConfig => adapterConfig;
    protected void SubmitCommandEvent(
        PrimeInvokeProperties invokeProperties,
        BaseCommandSource source,
        List<ParsedToken> tokens
        )
    {
        CommandEventBus.InvokeCommandEvent(invokeProperties.ImplementInvokeProperties(Id,Platform, Protocol, tokens), source);
    }
    public abstract void HandleCustomAction(string action, object paras);
    public abstract void HandleDefaultAction(DefaultActionType action, object paras);
}

[Serializable] public class AdapterConfigData : IEquatable<AdapterConfigData>
{
    public string EntryFile { get; set; } = "";
    public string Description { get; set; } = "";
    public string CommandPrefix { get; set; } = "!";
    public List<long> GroupIds { get; set; } = [];
    public JsonNode OriginalConfig { get; set; } = JsonNode.Parse("{}")!;

    public bool Equals(AdapterConfigData? other)
    {
        return EntryFile == other?.EntryFile &&
               Description == other.Description &&
               CommandPrefix == other.CommandPrefix;
    }
}
