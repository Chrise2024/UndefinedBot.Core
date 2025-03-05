using System.Text.Json.Nodes;

namespace UndefinedBot.Core.Utils;

public interface IReadonlyConfig
{
    public JsonNode? this[string key] { get; }
    public T? GetValue<T>(string key)  where T : notnull;
    public JsonNode? GetValue(string key);
}