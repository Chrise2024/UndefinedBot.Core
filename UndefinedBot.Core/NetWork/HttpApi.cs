using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                object reqJSON = new
                {
                    group_id = targetGroupId,
                    message = msgChain
                };
                await _httpClient.PostAsync(_httpPostUrl + "/send_group_msg",
                   new StringContent(
                       JsonConvert.SerializeObject(reqJSON),
                       Encoding.UTF8,
                       "application/json"
                   )
                );
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Task Canceled: ");
            }
            catch (Exception ex)
            {
                _httpApiLogger.Error("Error Occured, Error Information:");
                _httpApiLogger.Error(ex.Message);
                _httpApiLogger.Error(ex.StackTrace ?? "");
            }
        }
        public async Task SendGroupForward<T>(T targetGroupId,List<ForwardNode> forwardNodes)
        {
            try
            {
                object reqJSON = new
                {
                    group_id = targetGroupId,
                    message = forwardNodes
                };
                await _httpClient.PostAsync(_httpPostUrl + "/send_group_forward_msg",
                    new StringContent(
                        JsonConvert.SerializeObject(reqJSON),
                        Encoding.UTF8,
                        "application/json"
                    )
                );
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Task Canceled: ");
            }
            catch (Exception ex)
            {
                _httpApiLogger.Error("Error Occured, Error Information:");
                _httpApiLogger.Error(ex.Message);
                _httpApiLogger.Error(ex.StackTrace ?? "");
            }
        }
        public async void RecallGroupMsg<T>(T msgId)
        {
            try
            {
                object reqJSON = new
                {
                    message_id = msgId
                };
                await _httpClient.PostAsync(_httpPostUrl + "/delete_msg",
                   new StringContent(
                       JsonConvert.SerializeObject(reqJSON),
                       Encoding.UTF8,
                       "application/json"
                   )
                );
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Task Canceled: ");
            }
            catch (Exception ex)
            {
                _httpApiLogger.Error("Error Occured, Error Information:");
                _httpApiLogger.Error(ex.Message);
                _httpApiLogger.Error(ex.StackTrace ?? "");
            }
        }
        public async Task<MsgBodySchematics> GetMsg<T>(T msgId)
        {
            try
            {
                object reqJSON = new
                {
                    message_id = msgId
                };
                HttpResponseMessage response = await _httpClient.PostAsync(_httpPostUrl + "/get_msg",
                   new StringContent(
                       JsonConvert.SerializeObject(reqJSON),
                       Encoding.UTF8,
                       "application/json"
                   )
                );
                return JObject.Parse(response.Content.ReadAsStringAsync().Result)["data"]?.ToObject<MsgBodySchematics>() ?? new MsgBodySchematics();
            }
            catch (TaskCanceledException)
            {
                _httpApiLogger.Error("Task Canceled: ");
                return new MsgBodySchematics();
            }
            catch (Exception ex)
            {
                _httpApiLogger.Error("Error Occured, Error Information:");
                _httpApiLogger.Error(ex.Message);
                _httpApiLogger.Error(ex.StackTrace ?? "");
                return new MsgBodySchematics();
            }
        }
        public async Task<GroupMemberSchematics> GetGroupMember<T1, T2>(T1 targetGroupId, T2 targetUin)
        {
            try
            {
                object reqJSON = new
                {
                    group_id = targetGroupId,
                    user_id = targetUin,
                    no_cache = false
                };
                HttpResponseMessage response = await _httpClient.PostAsync(_httpPostUrl + "/get_group_member_info",
                   new StringContent(
                       JsonConvert.SerializeObject(reqJSON),
                       Encoding.UTF8,
                       "application/json"
                   )
                );
                return JObject.Parse(response.Content.ReadAsStringAsync().Result)["data"]?.ToObject<GroupMemberSchematics>() ?? new GroupMemberSchematics();
            }
            catch (TaskCanceledException)
            {
                _httpApiLogger.Error("Task Canceled: ");
                return new GroupMemberSchematics();
            }
            catch (Exception ex)
            {
                _httpApiLogger.Error("Error Occured, Error Information:");
                _httpApiLogger.Error(ex.Message);
                _httpApiLogger.Error(ex.StackTrace ?? "");
                return new GroupMemberSchematics();
            }
        }
        public async Task<List<GroupMemberSchematics>> GetGroupMemberList<T>(T targetGroupId)
        {
            try
            {
                object reqJSON = new
                {
                    group_id = targetGroupId,
                    no_cache = false
                };
                HttpResponseMessage response = await _httpClient.PostAsync(_httpPostUrl + "/get_group_member_list",
                    new StringContent(
                        JsonConvert.SerializeObject(reqJSON),
                        Encoding.UTF8,
                        "application/json"
                    )
                );
                return JObject.Parse(response.Content.ReadAsStringAsync().Result)["data"]?.ToObject<List<GroupMemberSchematics>>() ?? [];
            }
            catch (TaskCanceledException)
            {
                _httpApiLogger.Error("Task Canceled: ");
                return [];
            }
            catch (Exception ex)
            {
                _httpApiLogger.Error(ex.Message);
                _httpApiLogger.Error(ex.StackTrace ?? "");
                return [];
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
            Timeout = TimeSpan.FromSeconds(10)
        };

        private readonly Logger _httpRequestLogger = new("HttpRequest");

        public async Task<string> POST(string url, object? content = null)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(url, content == null ? null : new StringContent(
                       JsonConvert.SerializeObject(content),
                       Encoding.UTF8,
                       "application/json"
                   ));
                return await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException)
            {
                _httpRequestLogger.Error("Task Canceled: ");
                return "";
            }
            catch (Exception ex)
            {
                _httpRequestLogger.Error("Error Occured, Error Information:");
                _httpRequestLogger.Error(ex.Message);
                _httpRequestLogger.Error(ex.StackTrace ?? "");
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
            catch (TaskCanceledException)
            {
                _httpRequestLogger.Error("Task Canceled: ");
                return "";
            }
            catch (Exception ex)
            {
                _httpRequestLogger.Error("Error Occured, Error Information:");
                _httpRequestLogger.Error(ex.Message);
                _httpRequestLogger.Error(ex.StackTrace ?? "");
                return "";
            }
        }

        public async Task<byte[]> GetBinary(string url)
        {
            try
            {
                return await _httpClient.GetByteArrayAsync(url);
            }
            catch (TaskCanceledException)
            {
                _httpRequestLogger.Error("Task Canceled: ");
                return [];
            }
            catch (Exception ex)
            {
                _httpRequestLogger.Error("Error Occured, Error Information:");
                _httpRequestLogger.Error(ex.Message);
                _httpRequestLogger.Error(ex.StackTrace ?? "");
                return [];
            }
        }
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
