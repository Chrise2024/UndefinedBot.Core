namespace UndefinedBot.Core.Utils.Logging;

internal sealed class InternalLogger : BaseLogger
{
    protected override string Template { get; }
    protected override string[] Tags { get; }
    public InternalLogger(IEnumerable<string> tags)
    {
        Tags = tags.ToArray();
        string extendTemplate = "";
        for (int i = 0; i < Tags.Length; i++)
        {
            extendTemplate += $"[{{Tag{i}}}] ";
        }

        Template = "[{Time}] [Program] " + extendTemplate + "[{LogLevel}] {Message}";
    }
}