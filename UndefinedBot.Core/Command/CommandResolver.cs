using System.Text.RegularExpressions;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Command
{
    internal class CommandResolver
    {

        private static readonly CallingProperty s_noneCommandArg = new("", 0,0,0, "",0);

        private static readonly string s_commandPrefix = Core.GetConfigManager().GetCommandPrefix();

        private static readonly Logger s_argLogger = new("CommandResolver");

        /// <summary>
        /// Split mssage into tokens
        /// </summary>
        /// <param name="msgString">Raw CQMessage string</param>
        /// <returns>tokens</returns>
        public static (string?,List<string>) Tokenize(string msgString)
        {
            List<string> rawTokens = ParseCqString(msgString);
            if (rawTokens.Count > 1 && rawTokens[1].StartsWith(s_commandPrefix))
            {
                string cmd = rawTokens[1].Replace(s_commandPrefix,"");
                rawTokens.RemoveAt(1);
                return (cmd,rawTokens);
            }
            else if (rawTokens.Count > 0 && rawTokens[0].StartsWith(s_commandPrefix))
            {
                return (rawTokens[0].Replace(s_commandPrefix,""), rawTokens[1..]);
            }
            else
            {
                return (null, []);
            }
        }
        /*
        public static Arg Parse(MsgBody msgBody)
        {
            long groupId = msgBody.GroupId ?? 0;
            long callerUin = msgBody.UserId ?? 0;
            int msgId = msgBody.MessageId ?? 0;
            string CQString = msgBody.RawMessage ?? "";
            if (groupId == 0 || callerUin == 0 || msgId == 0)
            {
                s_argLogger.Error("Invalid Msg Body");
                return s_noneCommandArg;
            }
            else if (CQString.Length == 0)
            {
                s_argLogger.Error( "Raw Msg Is Null");
                return s_noneCommandArg;
            }
            else
            {
                s_argLogger.Info( "Resolving, Raw = " + CQString);
                Match matchCqReply = RegexProvider.GetCQReplyRegex().Match(CQString);
                if (matchCqReply.Success)
                {
                    CQEntity cqEntity = DecodeCqEntity(matchCqReply.Value);
                    int targetMsgId = Int32.Parse(cqEntity.Properties.GetValueOrDefault("id", "0"));
                    string normalCqString = CQString.Replace(matchCqReply.Value, "").Trim();
                    if ( normalCqString.StartsWith(s_commandPrefix))
                    {
                        List<string> Params = ParseCqString(normalCqString[s_commandPrefix.Length..]);
                        s_argLogger.Info( "Parse Complete");
                        return new Arg(
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
                    List<string> Params = ParseCqString(CQString[s_commandPrefix.Length..]);
                    s_argLogger.Info( "Parse Complete");
                    return new Arg(
                            Params[0],
                            Params[1..],
                            callerUin,
                            groupId,
                            msgId,
                            true
                            );
                }
                s_argLogger.Info( "Parse Complete");
            }
            return s_noneCommandArg;
        }
        */
        private static List<string> ParseCqString(string cqString)
        {
            if (cqString.Length == 0)
            {
                return [];
            }
            return
            [
                ..RegexProvider.GetCQEntityRegex().Replace(
                    cqString, match => $" {match.Value} "
                    /*
                    {
                        CQEntity cqEntity = DecodeCqEntity(match.Value);
                        if (cqEntity.CQType.Equals("at"))
                        {
                            if (cqEntity.Properties.TryGetValue("qq", out string? uin))
                            {
                                return $" {uin} ";
                            }
                        }
                        else if (cqEntity.CQType.Equals("reply"))
                        {
                            if (cqEntity.Properties.TryGetValue("id", out string? msgId))
                            {
                                return $" {msgId} ";
                            }
                        }
                        else if (cqEntity.CQType.Equals("image"))
                        {
                            if (cqEntity.Properties.TryGetValue("url", out string? imageUrl))
                            {
                                return $" {imageUrl} ";
                            }
                            else
                            {
                                return cqEntity.Properties.TryGetValue("file", out string? imUrl) ? $" {imUrl} " : " ";
                            }
                        }

                        return " ";
                    }*/
                ).Split(" ", StringSplitOptions.RemoveEmptyEntries)

            ];
        }
        public static CQEntity DecodeCqEntity(string cqEntityString)
        {
            Dictionary<string,string> properties = [];
            cqEntityString = cqEntityString[1..^1]
                .Replace(",", "\r$\r")
                .Replace("&amp;", "&")
                .Replace("&#91;", "[")
                .Replace("&#93;", "]")
                .Replace("&#44;", ",");
            string[] cqPiece = cqEntityString.Split("\r$\r");
            CQEntity cqEntity = new(cqPiece[0][3..]);
            for (int i = 1; i < cqPiece.Length; i++)
            {
                string[] temp = cqPiece[i].Split("=",2,StringSplitOptions.RemoveEmptyEntries);
                if (temp.Length > 1)
                {

                    properties.Add(temp[0], temp[1]);
                }
            }
            cqEntity.Properties = properties;
            return cqEntity;
        }
        /*
        private static string ExtractUrlFromMsg(MsgBody msgBody)
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
    public struct CallingProperty(
        string command,
        long callerUin,
        long groupId,
        int msgId,
        string subType,
        long time
        )
    {
        public string Command = command;
        public long CallerUin = callerUin;
        public long GroupId = groupId;
        public int MsgId = msgId;
        public string SubType = subType;
        public long Time = time;
    }
    public struct CQEntity(string cqType)
    {
        public string CQType = cqType;
        public Dictionary<string, string> Properties;
    }
}
