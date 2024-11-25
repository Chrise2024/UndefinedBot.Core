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
        /// <summary>
        /// Send Message to Group
        /// </summary>
        /// <param name="targetGroupId">Group Id to send</param>
        /// <param name="msgChain">Onebot11 MessageChain</param>
        /// <typeparam name="T">string or long</typeparam>
        public async Task SendGroupMsg<T>(T targetGroupId,List<JObject> msgChain)
        {
            try
            {
                object reqJson = new
                {
                    group_id = targetGroupId,
                    message = msgChain
                };
                await _httpClient.PostAsync(_httpPostUrl + "/send_group_msg",
                   new StringContent(
                       JsonConvert.SerializeObject(reqJson),
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
        /// <summary>
        /// Send Forward to Group
        /// </summary>
        /// <param name="targetGroupId">Group Id to send</param>
        /// <param name="forwardNodes">Onebot11 MessageChain</param>
        /// <typeparam name="T">string or long</typeparam>
        public async Task SendGroupForward<T>(T targetGroupId,List<ForwardNode> forwardNodes)
        {
            try
            {
                object reqJson = new
                {
                    group_id = targetGroupId,
                    message = forwardNodes
                };
                await _httpClient.PostAsync(_httpPostUrl + "/send_group_forward_msg",
                    new StringContent(
                        JsonConvert.SerializeObject(reqJson),
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
        /// <summary>
        /// Recall Message in Group
        /// </summary>
        /// <param name="msgId">Message Id of Message to Recall</param>
        /// <typeparam name="T">string or int</typeparam>
        public async void RecallGroupMsg<T>(T msgId)
        {
            try
            {
                object reqJson = new
                {
                    message_id = msgId
                };
                await _httpClient.PostAsync(_httpPostUrl + "/delete_msg",
                   new StringContent(
                       JsonConvert.SerializeObject(reqJson),
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
        /// <summary>
        /// Get Message
        /// </summary>
        /// <param name="msgId">Message Id of Message to get</param>
        /// <typeparam name="T">string or int</typeparam>
        /// <returns>MsgBody</returns>
        public async Task<MsgBody> GetMsg<T>(T msgId)
        {
            try
            {
                object reqJson = new
                {
                    message_id = msgId
                };
                HttpResponseMessage response = await _httpClient.PostAsync(_httpPostUrl + "/get_msg",
                   new StringContent(
                       JsonConvert.SerializeObject(reqJson),
                       Encoding.UTF8,
                       "application/json"
                   )
                );
                return JObject.Parse(response.Content.ReadAsStringAsync().Result)["data"]?.ToObject<MsgBody>() ?? new MsgBody();
            }
            catch (TaskCanceledException)
            {
                _httpApiLogger.Error("Task Canceled: ");
                return new MsgBody();
            }
            catch (Exception ex)
            {
                _httpApiLogger.Error("Error Occured, Error Information:");
                _httpApiLogger.Error(ex.Message);
                _httpApiLogger.Error(ex.StackTrace ?? "");
                return new MsgBody();
            }
        }
        /// <summary>
        /// Get group member info
        /// </summary>
        /// <param name="targetGroupId">Group Id</param>
        /// <param name="targetUin">User Id to get</param>
        /// <typeparam name="T1">string or long</typeparam>
        /// <typeparam name="T2">string or long</typeparam>
        /// <returns>GroupMember</returns>
        public async Task<GroupMember> GetGroupMember<T1, T2>(T1 targetGroupId, T2 targetUin)
        {
            try
            {
                object reqJson = new
                {
                    group_id = targetGroupId,
                    user_id = targetUin,
                    no_cache = false
                };
                HttpResponseMessage response = await _httpClient.PostAsync(_httpPostUrl + "/get_group_member_info",
                   new StringContent(
                       JsonConvert.SerializeObject(reqJson),
                       Encoding.UTF8,
                       "application/json"
                   )
                );
                return JObject.Parse(response.Content.ReadAsStringAsync().Result)["data"]?.ToObject<GroupMember>() ?? new GroupMember();
            }
            catch (TaskCanceledException)
            {
                _httpApiLogger.Error("Task Canceled: ");
                return new GroupMember();
            }
            catch (Exception ex)
            {
                _httpApiLogger.Error("Error Occured, Error Information:");
                _httpApiLogger.Error(ex.Message);
                _httpApiLogger.Error(ex.StackTrace ?? "");
                return new GroupMember();
            }
        }

        /// <summary>
        /// Get group member list
        /// </summary>
        /// <param name="targetGroupId">Group Id to get</param>
        /// <typeparam name="T">string or long</typeparam>
        /// <returns>A list contains GroupMember</returns>
        public async Task<List<GroupMember>> GetGroupMemberList<T>(T targetGroupId)
        {
            try
            {
                object reqJson = new
                {
                    group_id = targetGroupId,
                    no_cache = false
                };
                HttpResponseMessage response = await _httpClient.PostAsync(_httpPostUrl + "/get_group_member_list",
                    new StringContent(
                        JsonConvert.SerializeObject(reqJson),
                        Encoding.UTF8,
                        "application/json"
                    )
                );
                return JObject.Parse(response.Content.ReadAsStringAsync().Result)["data"]?.ToObject<List<GroupMember>>() ?? [];
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

        public async Task<string> Post(string url, object? content = null)
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
                return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : "";
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
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : [];
                //return await _httpClient.GetByteArrayAsync(url);
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
    public struct GroupMember
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
