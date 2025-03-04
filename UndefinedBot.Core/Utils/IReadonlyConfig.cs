namespace UndefinedBot.Core.Utils;

internal interface IReadonlyConfig
{
    public string? this[string key] { get; }
    public T? GetValue<T>(string key)  where T : notnull;
}