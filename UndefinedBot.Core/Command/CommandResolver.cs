using System;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UndefinedBot.Core.NetWork;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Command
{
    internal class CommandResolver
    {
        private static readonly ArgSchematics s_invalidCommandArg = new("null", [], 0,  0, 0, false);

        private static readonly ArgSchematics s_noneCommandArg = new("", [],0,0,0, false);

        private static readonly List<long> s_workGRoup = Core.GetConfigManager().GetGroupList();

        private static readonly string s_commandPrefix = Core.GetConfigManager().GetCommandPrefix();

        private static readonly Logger s_argLogger = new("CommandResolver");

        private static readonly Logger s_handleLogger = new("CommandResolver");
        public static ArgSchematics Parse(MsgBodySchematics msgBody)
        {
            long GroupId = msgBody.GroupId ?? 0;
            long CallerUin = msgBody.UserId ?? 0;
            int MsgId = msgBody.MessageId ?? 0;
            string CQString = msgBody.RawMessage ?? "";
            if (MsgId == 0)
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
                Match MatchCQReply = RegexProvider.GetCQReplyRegex().Match(CQString);
                if (MatchCQReply.Success)
                {
                    CQEntitySchematics CQEntity = DecodeCQEntity(MatchCQReply.Value);
                    int TargetMsgId = Int32.Parse(CQEntity.Properties.TryGetValue("id", out var IntMsgId) ? IntMsgId : "0");
                    string NormalCQString = CQString.Replace(MatchCQReply.Value, "").Trim();
                    if ( NormalCQString.StartsWith(s_commandPrefix))
                    {
                        List<string> Params = ParseCQString(NormalCQString[s_commandPrefix.Length..]);
                        s_argLogger.Info("ArgParse", "Parse Complete");
                        return new ArgSchematics(
                            Params[0],
                            [$"{TargetMsgId}", ..Params[1..]],
                            CallerUin,
                            GroupId,
                            MsgId,
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
                            CallerUin,
                            GroupId,
                            MsgId,
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
            return new(
                RegexProvider.GetCQEntityRegex().Replace(
                    CQString, match => {
                        CQEntitySchematics CQEntity = DecodeCQEntity(match.Value);
                        if (CQEntity.CQType.Equals("at"))
                        {
                            if (CQEntity.Properties.TryGetValue("qq",out var Uin))
                            {
                                return $" {Uin} ";
                            }
                        }
                        else if (CQEntity.CQType.Equals("reply"))
                        {
                            if (CQEntity.Properties.TryGetValue("id", out var MsgId))
                            {
                                return $" {MsgId} ";
                            }
                        }
                        else if (CQEntity.CQType.Equals("image"))
                        {
                            if (CQEntity.Properties.TryGetValue("url", out var ImageUrl))
                            {
                                return $" {ImageUrl} ";
                            }
                            else
                            {
                                return CQEntity.Properties.TryGetValue("file", out var IUrl) ? $" {IUrl} " : " ";
                            }
                        }
                        return " ";
                    }
                ).Split(" ", StringSplitOptions.RemoveEmptyEntries)
            );
        }
        private static CQEntitySchematics DecodeCQEntity(string CQEntityString)
        {
            Dictionary<string,string> Properties = [];
            CQEntityString = CQEntityString[1..(CQEntityString.Length - 1)]
                .Replace(",", "\r")
                .Replace("&amp;", "&")
                .Replace("&#91;", "[")
                .Replace("&#93;", "]")
                .Replace("&#44;", ",");
            string[] CQPiece = CQEntityString.Split("\r");
            CQEntitySchematics CQEntity = new(CQPiece[0][3..]);
            for (int i = 1; i < CQPiece.Length; i++)
            {
                string[] temp = CQPiece[i].Split("=",2,StringSplitOptions.RemoveEmptyEntries);
                if (temp.Length > 1)
                {

                    Properties.Add(temp[0], temp[1]);
                }
            }
            CQEntity.Properties = Properties;
            return CQEntity;
        }

        public static string ExtractUrlFromMsg(MsgBodySchematics msgBody)
        {
            if (msgBody.Message?.Count > 0)
            {
                List<JObject> MsgChain = msgBody.Message;
                if (MsgChain.Count > 0)
                {
                    JObject Msg = MsgChain[0];
                    if (Msg.Value<string>("type")?.Equals("image") ?? false)
                    {
                        if (Msg.TryGetValue("data", out var JT))
                        {
                            JObject? DataObj = JT.ToObject<JObject>();
                            if (DataObj != null)
                            {
                                if (DataObj.TryGetValue("url",out var Temp))
                                {
                                    return Temp.ToObject<string>() ?? "";
                                }
                                else
                                {
                                    return DataObj.Value<string>("file") ?? "";
                                }
                            }
                        }
                    }
                }
            }
            return "";
        }
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
