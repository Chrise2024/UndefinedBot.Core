using System.Text.RegularExpressions;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Command
{
    internal class CommandResolver
    {

        private static readonly ArgSchematics s_noneCommandArg = new("", [],0,0,0, false);

        private static readonly string s_commandPrefix = Core.GetConfigManager().GetCommandPrefix();

        private static readonly Logger s_argLogger = new("CommandResolver");
        public static ArgSchematics Parse(MsgBodySchematics msgBody)
        {
            long groupId = msgBody.GroupId ?? 0;
            long callerUin = msgBody.UserId ?? 0;
            int msgId = msgBody.MessageId ?? 0;
            string CQString = msgBody.RawMessage ?? "";
            if (groupId == 0 || callerUin == 0 || msgId == 0)
            {
                s_argLogger.Error("ArgParse","Invalid Msg Body");
                return s_noneCommandArg;
            }
            else if (CQString.Length == 0)
            {
                s_argLogger.Error("ArgParse", "Raw Msg Is Null");
                return s_noneCommandArg;
            }
            else
            {
                s_argLogger.Info("ArgParse", "Resolving, Raw = " + CQString);
                Match matchCQReply = RegexProvider.GetCQReplyRegex().Match(CQString);
                if (matchCQReply.Success)
                {
                    CQEntitySchematics CQEntity = DecodeCQEntity(matchCQReply.Value);
                    int targetMsgId = Int32.Parse(CQEntity.Properties.GetValueOrDefault("id", "0"));
                    string normalCQString = CQString.Replace(matchCQReply.Value, "").Trim();
                    if ( normalCQString.StartsWith(s_commandPrefix))
                    {
                        List<string> Params = ParseCQString(normalCQString[s_commandPrefix.Length..]);
                        s_argLogger.Info("ArgParse", "Parse Complete");
                        return new ArgSchematics(
                            Params[0],
                            [$"{targetMsgId}", ..Params[1..]],
                            callerUin,
                            groupId,
                            msgId,
                            true
                            );
                    }
                }
                else if (CQString.StartsWith(s_commandPrefix) && !CQString.Equals(s_commandPrefix))
                {
                    List<string> Params = ParseCQString(CQString[s_commandPrefix.Length..]);
                    s_argLogger.Info("ArgParse", "Parse Complete");
                    return new ArgSchematics(
                            Params[0],
                            Params[1..],
                            callerUin,
                            groupId,
                            msgId,
                            true
                            );
                }
                s_argLogger.Info("ArgParse", "Parse Complete");
            }
            return s_noneCommandArg;
        }

        private static List<string> ParseCQString(string CQString)
        {
            if (CQString.Length == 0)
            {
                return [];
            }
            return
            [
                ..RegexProvider.GetCQEntityRegex().Replace(
                    CQString, match =>
                    {
                        CQEntitySchematics CQEntity = DecodeCQEntity(match.Value);
                        if (CQEntity.CQType.Equals("at"))
                        {
                            if (CQEntity.Properties.TryGetValue("qq", out string? uin))
                            {
                                return $" {uin} ";
                            }
                        }
                        else if (CQEntity.CQType.Equals("reply"))
                        {
                            if (CQEntity.Properties.TryGetValue("id", out string? msgId))
                            {
                                return $" {msgId} ";
                            }
                        }
                        else if (CQEntity.CQType.Equals("image"))
                        {
                            if (CQEntity.Properties.TryGetValue("url", out string? imageUrl))
                            {
                                return $" {imageUrl} ";
                            }
                            else
                            {
                                return CQEntity.Properties.TryGetValue("file", out string? imUrl) ? $" {imUrl} " : " ";
                            }
                        }

                        return " ";
                    }
                ).Split(" ", StringSplitOptions.RemoveEmptyEntries)

            ];
        }
        private static CQEntitySchematics DecodeCQEntity(string CQEntityString)
        {
            Dictionary<string,string> properties = [];
            CQEntityString = CQEntityString[1..^1]
                .Replace(",", "\r$\r")
                .Replace("&amp;", "&")
                .Replace("&#91;", "[")
                .Replace("&#93;", "]")
                .Replace("&#44;", ",");
            string[] CQPiece = CQEntityString.Split("\r$\r");
            CQEntitySchematics CQEntity = new(CQPiece[0][3..]);
            for (int i = 1; i < CQPiece.Length; i++)
            {
                string[] temp = CQPiece[i].Split("=",2,StringSplitOptions.RemoveEmptyEntries);
                if (temp.Length > 1)
                {

                    properties.Add(temp[0], temp[1]);
                }
            }
            CQEntity.Properties = properties;
            return CQEntity;
        }
        /*
        private static string ExtractUrlFromMsg(MsgBodySchematics msgBody)
        {
            if (msgBody.Message?.Count > 0)
            {
                List<JObject> msgChain = msgBody.Message;
                if (msgChain.Count > 0)
                {
                    JObject msg = msgChain[0];
                    if (msg.Value<string>("type")?.Equals("image") ?? false)
                    {
                        if (msg.TryGetValue("data", out var JT))
                        {
                            JObject? dataObj = JT.ToObject<JObject>();
                            if (dataObj != null)
                            {
                                if (dataObj.TryGetValue("url",out var temp))
                                {
                                    return temp.ToObject<string>() ?? "";
                                }
                                else
                                {
                                    return dataObj.Value<string>("file") ?? "";
                                }
                            }
                        }
                    }
                }
            }
            return "";
        }
        */
    }
    public struct ArgSchematics(
        string command,
        List<string> param,
        long callerUin,
        long groupId,
        int msgId,
        bool status
        )
    {
        public string Command = command;
        public List<string> Param = param;
        public long CallerUin = callerUin;
        public long GroupId = groupId;
        public int MsgId = msgId;
        public bool Status = status;
    }
    public struct CQEntitySchematics(string CQType)
    {
        public string CQType = CQType;
        public Dictionary<string, string> Properties;
    }
}
