using UndefinedBot.Core.Utils;

namespace UndefinedBot.Net.Utils.Logging;

internal sealed class InternalLogger : BaseLogger
{
    protected override string Template { get; }
    protected override string[] Tags { get; }
    protected override Microsoft.Extensions.Logging.ILogger<InternalLogger> RootLogger { get; }

    public InternalLogger(Microsoft.Extensions.Logging.ILogger<InternalLogger> rootLogger, IEnumerable<string> tags)
    {
        RootLogger = rootLogger;
        Tags = tags.ToArray();
        string extendTemplate = "";
        for (int i = 0; i < Tags.Length; i++) extendTemplate += $"[{{Tag{i}}}] ";

        Template = "[{Time}] [Program] " + extendTemplate + "[{LogLevel}] {Message}";
    }

    public override ILogger Extend(string subSpace)
    {
        throw new NotSupportedException();
    }

    public override ILogger Extend(IEnumerable<string> subSpace)
    {
        throw new NotSupportedException();
    }
}