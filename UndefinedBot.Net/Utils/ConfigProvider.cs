using System.Text.Json;
using System.Text.Json.Nodes;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Net.Utils;

internal class ConfigProvider(JsonNode originalConfig) : IReadonlyConfig
{
    private JsonNode OriginalConfig { get; } = originalConfig;

    public JsonNode? this[string key]
    {
        get
        {
            JsonNode temp = OriginalConfig;
            string[] keys = key.Split(':');
            foreach (string k in keys)
            {
                JsonNode? next = temp[k];
                if (next is null)
                {
                    Console.WriteLine(temp);
                    return temp;
                }
                temp = next;
            }

            return temp;
        }
    }

    public T? GetValue<T>(string key) where T : notnull
    {
        JsonNode? value = this[key];
        return value is null ? default : value.Deserialize<T>();
    }
    
    public JsonNode? GetValue(string key) => this[key];

}