using System;
using System.Text;
using System.Net;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UndefinedBot.Core;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;

namespace UndefinedBot.Core.NetWork
{
    public class HttpApi(string httpPostUrl)
    {
        private readonly string _httpPostUrl = httpPostUrl;

        private readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(20)
        };

        private readonly Logger _httpApiLogger = new("HttpRequest");

        public async Task SendGroupMsg<T>(T targetGroupId,List<JObject> msgChain)
        {
            try
            {
                object ReqJSON = new
                {
                    group_id = targetGroupId,
                    message = msgChain
                };
                //Console.WriteLine(JsonConvert.SerializeObject(ReqJSON));
                await _httpClient.PostAsync(_httpPostUrl + "/send_group_msg",
                   new StringContent(
                       JsonConvert.SerializeObject(ReqJSON),
                       Encoding.UTF8,
                       "application/json"
                   )
                );
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Task Cacled: ");
                _httpApiLogger.Error("HttpApi",ex.Message);
                _httpApiLogger.Error("HttpApi", ex.StackTrace ?? "");
            }
            catch (Exception ex)
            {
                _httpApiLogger.Error("HttpApi", "Error Occured, Error Information:");
                _httpApiLogger.Error("HttpApi", ex.Message);
                _httpApiLogger.Error("HttpApi", ex.StackTrace ?? "");
            }
        }
        public async void RecallGroupMsg<T>(T msgId)
        {
            try
            {
                object ReqJSON = new
                {
                    message_id = msgId
                };
                await _httpClient.PostAsync(_httpPostUrl + "/delete_msg",
                   new StringContent(
                       JsonConvert.SerializeObject(ReqJSON),
                       Encoding.UTF8,
                       "application/json"
                   )
                );
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Task Cacled: ");
                _httpApiLogger.Error("HttpApi", ex.Message);
                _httpApiLogger.Error("HttpApi", ex.StackTrace ?? "");
            }
            catch (Exception ex)
            {
                _httpApiLogger.Error("HttpApi", "Error Occured, Error Information:");
                _httpApiLogger.Error("HttpApi", ex.Message);
                _httpApiLogger.Error("HttpApi", ex.StackTrace ?? "");
            }
        }
        public async Task<MsgBodySchematics> GetMsg<T>(T msgId)
        {
            try
            {
                object ReqJSON = new
                {
                    message_id = msgId
                };
                HttpResponseMessage response = await _httpClient.PostAsync(_httpPostUrl + "/get_msg",
                   new StringContent(
                       JsonConvert.SerializeObject(ReqJSON),
                       Encoding.UTF8,
                       "application/json"
                   )
                );
                return JObject.Parse(response.Content.ReadAsStringAsync().Result)["data"]?.ToObject<MsgBodySchematics>() ?? new MsgBodySchematics();
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Task Cacled: ");
                _httpApiLogger.Error("HttpApi", ex.Message);
                _httpApiLogger.Error("HttpApi", ex.StackTrace ?? "");
                return new MsgBodySchematics();
            }
            catch
            {
                return new MsgBodySchematics();
            }
        }
        public async Task<GroupMemberSchematics> GetGroupMember<T1, T2>(T1 targetGroupId, T2 targetUin)
        {
            try
            {
                object ReqJSON = new
                {
                    group_id = targetGroupId,
                    user_id = targetUin,
                    no_cache = false
                };
                HttpResponseMessage response = await _httpClient.PostAsync(_httpPostUrl + "/get_group_member_info",
                   new StringContent(
                       JsonConvert.SerializeObject(ReqJSON),
                       Encoding.UTF8,
                       "application/json"
                   )
                );
                return JObject.Parse(response.Content.ReadAsStringAsync().Result)["data"]?.ToObject<GroupMemberSchematics>() ?? new GroupMemberSchematics();
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Task Cacled: ");
                _httpApiLogger.Error("HttpApi", ex.Message);
                _httpApiLogger.Error("HttpApi", ex.StackTrace ?? "");
                return new GroupMemberSchematics();
            }
            catch
            {
                return new GroupMemberSchematics();
            }
        }
        public async Task<HitokotoSchematics> GetHitokoto(string htioType)
        {
            string Para = "";
            foreach(char index in htioType)
            {
                if (index >= 'a' && index <= 'l')
                {
                    Para += $"c={index}&";
                }
            }
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("https://v1.hitokoto.cn/?" + Para);
                return JsonConvert.DeserializeObject<HitokotoSchematics>(response.Content.ReadAsStringAsync().Result);
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Task Cacled: ");
                _httpApiLogger.Error("HttpApi", ex.Message);
                _httpApiLogger.Error("HttpApi", ex.StackTrace ?? "");
                return new HitokotoSchematics();
            }
            catch
            {
                return new HitokotoSchematics();
            }
        }
        public async Task<bool> CheckUin(long targetGroupId, long targetUin)
        {
            return ((await GetGroupMember(targetGroupId, targetUin)).GroupId ?? 0) != 0;
        }
    }
    public class HttpRequest
    {
        private readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        private readonly Logger _httpRequestLogger = new("HttpRequest");

        public async Task<string> POST(string Url, object? content = null)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(Url, content == null ? null : new StringContent(
                       JsonConvert.SerializeObject(content),
                       Encoding.UTF8,
                       "application/json"
                   ));
                return await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Task Cacled: ");
                _httpRequestLogger.Error("Request", ex.Message);
                _httpRequestLogger.Error("Request", ex.StackTrace ?? "");
                return "";
            }
            catch (Exception ex)
            {
                _httpRequestLogger.Error("Request", "Error Occured, Error Information:");
                _httpRequestLogger.Error("Request", ex.Message);
                _httpRequestLogger.Error("Request", ex.StackTrace ?? "");
                return "";
            }
        }

        public async Task<string> Get(string url)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                return await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Task Cacled: ");
                _httpRequestLogger.Error("Request", ex.Message);
                _httpRequestLogger.Error("Request", ex.StackTrace ?? "");
                return "";
            }
            catch (Exception ex)
            {
                _httpRequestLogger.Error("Request", "Error Occured, Error Information:");
                _httpRequestLogger.Error("Request", ex.Message);
                _httpRequestLogger.Error("Request", ex.StackTrace ?? "");
                return "";
            }
        }

        public async Task<byte[]> GetBinary(string url)
        {
            try
            {
                return await _httpClient.GetByteArrayAsync(url);
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Task Cacled: ");
                _httpRequestLogger.Error("Request", ex.Message);
                _httpRequestLogger.Error("Request", ex.StackTrace ?? "");
                return [];
            }
            catch (Exception ex)
            {
                _httpRequestLogger.Error("Request", "Error Occured, Error Information:");
                _httpRequestLogger.Error("Request", ex.Message);
                _httpRequestLogger.Error("Request", ex.StackTrace ?? "");
                return [];
            }
        }
    }
    public struct HitokotoSchematics
    {
        [JsonProperty("id")] public int? Id;
        [JsonProperty("uuid")] public string? Uuid;
        [JsonProperty("hitokoto")] public string? Hitokoto;
        [JsonProperty("type")] public string? Type;
        [JsonProperty("from")] public string? From;
        [JsonProperty("from_who")] public string? FromWho;
        [JsonProperty("creator")] public string? Creator;
        [JsonProperty("creator_uid")] public int? CreatorUid;
        [JsonProperty("reviewer")] public int? Reviewer;
        [JsonProperty("commit_from")] public string? CommitFrom;
        [JsonProperty("created_at")] public string? CreatedAt;
        [JsonProperty("length")] public int? Length;
    }
    public struct GroupMemberSchematics
    {
        [JsonProperty("group_id")] public long? GroupId;
        [JsonProperty("user_id")] public long? UserId;
        [JsonProperty("nickname")] public string? Nickname;
        [JsonProperty("card")] public string? Card;
        [JsonProperty("sex")] public string? Sex;
        [JsonProperty("age")] public int? Age;
        [JsonProperty("area")] public string? Area;
        [JsonProperty("join_time")] public int? JoinTime;
        [JsonProperty("last_sent_time")] public int? LastSentTime;
        [JsonProperty("level")] public string? Level;
        [JsonProperty("role")] public string? Role;
        [JsonProperty("unfriendly")] public bool? Unfriendly;
        [JsonProperty("title")] public string? Title;
        [JsonProperty("title_expire_time")] public int? TitleExpireTime;
        [JsonProperty("card_changeable")] public bool? CardChangeable;
    }
}
