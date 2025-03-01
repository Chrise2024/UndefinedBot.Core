namespace UndefinedBot.Core.Utils;

public interface ILogger
{
    void Critical(string message);
    void Critical(Exception? ex, string message);
    void Error(string message);
    void Error(Exception? ex, string message);
    void Warn(string message);
    void Warn(Exception? ex, string message);
    void Info(string message);
    void Info(Exception? ex, string message);
    void Debug(string message);
    void Debug(Exception? ex, string message);
    void Trace(string message);
    void Trace(Exception? ex, string message);
    ILogger Extend(string subSpace);
    ILogger Extend(IEnumerable<string> subSpace);
}