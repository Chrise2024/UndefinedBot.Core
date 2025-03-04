using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Net.Utils;

internal class ConfigProvider(IConfiguration configuration) : IReadonlyConfig
{
    public string? this[string key] => configuration[key];

    public T? GetValue<T>(string key) where T : notnull
    {
        string? value = configuration[key];
        return value is null ? default : JsonSerializer.Deserialize<T>(value);
    }
}