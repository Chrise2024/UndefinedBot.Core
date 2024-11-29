using System.Text.RegularExpressions;
using UndefinedBot.Core.Command.Arguments.ArgumentRange;

namespace UndefinedBot.Core.Command.Arguments.ArgumentType
{
    public class ImageArgument : IArgumentType
    {
        public string TypeName => "Image";
        public IArgumentRange? Range => null;
        public bool IsValid(string token)
        {
            return QImage.CqImageRegex().IsMatch(token);
        }
        public object GetValue(string token)
        {
            return QImage.Parse(token);
        }
        public static QImage GetImage(string key,CommandContext ctx)
        {
            string token = ctx.ArgumentReference.GetValueOrDefault(key) ??
                           throw new ArgumentInvalidException($"Undefined Argument: {key}");
            return QImage.Parse(token);
        }
    }
    public partial class QImage
    {
        public readonly string File;
        public readonly string? Url;
        public readonly string? Type;
        private QImage(string fl,string? url = null,string? type = null)
        {
            File = fl;
            Url = url;
            Type = type;
        }

        public static QImage Parse(string token)
        {
            if (CqImageRegex().IsMatch(token))
            {
                CqEntity entity = CommandResolver.DecodeCqEntity(token);
                if (entity.Properties.TryGetValue("file", out string? fl))
                {
                    return new QImage(fl, entity.Properties.GetValueOrDefault("url"),entity.Properties.GetValueOrDefault("type"));
                }
                else
                {
                    throw new ArgumentInvalidException($"{token} Is Not Valid Image");
                }
            }
            else
            {
                throw new ArgumentInvalidException($"{token} Is Not Valid Image");
            }
        }
        [GeneratedRegex(@"^\[CQ:image\S*\]$")]
        public static partial Regex CqImageRegex();
    }
}
