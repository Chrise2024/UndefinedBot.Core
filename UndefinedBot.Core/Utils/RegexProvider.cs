using System;
using System.Text.RegularExpressions;

namespace UndefinedBot.Core.Utils
{
    internal partial class RegexProvider
    {
        [GeneratedRegex(@"\[CQ:\S+\]")]
        public static partial Regex GetCQEntityRegex();

        [GeneratedRegex(@"\[CQ:at,qq=\d+\S*\]")]
        public static partial Regex GetCQAtRegex();

        [GeneratedRegex(@"^\[CQ:reply,id=[-]*\d+\]")]
        public static partial Regex GetCQReplyRegex();

        [GeneratedRegex(@"\d+")]
        public static partial Regex GetIdRegex();

        [GeneratedRegex(@"^-*\d+$")]
        public static partial Regex GetIntegerRegex();
    }
}
