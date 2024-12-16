
namespace UndefinedBot.Core.Command.Arguments;

[Serializable] public class ParsedToken
{
    public RawTokenTypes TokenType { get; set; } = RawTokenTypes.NormalContent;
    public string Content { get; set; } = "";
}

public enum RawTokenTypes
{
    NormalContent = 0,
    CqCodeContent = 1,
}
