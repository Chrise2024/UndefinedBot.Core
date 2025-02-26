namespace UndefinedBot.Core.Utils.Logging;

internal readonly struct LogMessage(
    UndefinedLogLevel logLevel,
    string template,
    object[] content,
    Exception? exception = null)
{
    public readonly UndefinedLogLevel LogLevel = logLevel;
    public readonly string Template = template;
    public readonly object[] Content = content;
    public readonly Exception? Exception = exception;
}