using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using UndefinedBot.Core.Adapter;
using UndefinedBot.Core.Utils;

namespace Adapter.OneBot11;

public class HttpApi
{
    private string HttpPostUrl { get; }
    public HttpApi(AdapterConfigData adapterConfig)
    {
        HttpServiceOptions? postConfig = adapterConfig.OriginalConfig["Post"]?.Deserialize<HttpServiceOptions>();
        if (postConfig == null)
        {
            throw new Exception("Server Properties Not Implemented");
        }
        HttpClient.DefaultRequestHeaders.Add("Authorization", postConfig.AccessToken);
        HttpPostUrl = $"http://{postConfig.Host}:{postConfig.Port}";
    }
    private static HttpClient HttpClient => new()
    {
        Timeout = TimeSpan.FromSeconds(20)
    };

    private readonly GeneralLogger _httpApiLogger = new("HttpRequest");
    /// <summary>
    /// Send Message to Group
    /// </summary>
    /// <param name="targetGroupId">Group Id to send</param>
    /// <param name="msgChain">Onebot11 MessageChain</param>
    public async Task SendGroupMsg(object targetGroupId,List<JsonNode> msgChain)
    {
        await ApiPostRequestWithoutResponse("/send_group_msg", new
        {
            group_id = targetGroupId,
            message = msgChain
        });
    }
    /// <summary>
    /// Send Forward to Group
    /// </summary>
    /// <param name="targetGroupId">Group Id to send</param>
    /// <param name="forwardPropertiesData">Onebot11 Forward MessageChain</param>
    public async Task SendGroupForward(object targetGroupId,ForwardPropertiesData forwardPropertiesData)
    {
        //JsonNode reqJson = JsonNode.FromObject(forwardSummaryData);
        JsonNode reqJson = JsonSerializer.SerializeToNode(forwardPropertiesData)!;
        reqJson["group_id"] = $"{targetGroupId}";
        await ApiPostRequestWithoutResponse("/send_group_forward_msg", reqJson);
    }
    /// <summary>
    /// Recall Message in Group
    /// </summary>
    /// <param name="msgId">Message Id of Message to Recall</param>
    public async void RecallGroupMsg(object msgId)
    {
        await ApiPostRequestWithoutResponse("/delete_msg", new
        {
            message_id = msgId
        });
    }
    /// <summary>
    /// Get Message
    /// </summary>
    /// <param name="msgId">Message Id of Message to get</param>
    /// <returns>MsgBody</returns>
    public async Task<MsgBody?> GetMsg(object msgId)
    {
        return await ApiPostRequestWithResponse<MsgBody>("/get_msg", new
        {
            message_id = msgId
        });
    }

    /// <summary>
    /// Get group member info
    /// </summary>
    /// <param name="targetGroupId">Group Id</param>
    /// <param name="targetUin">User Id to get</param>
    /// <returns>GroupMember</returns>
    public async Task<GroupMember?> GetGroupMember(object targetGroupId, object targetUin)
    {
        return await ApiPostRequestWithResponse<GroupMember>("/get_group_member_info", new
        {
            group_id = targetGroupId,
            user_id = targetUin,
            no_cache = false
        });
    }

    /// <summary>
    /// Get group member list
    /// </summary>
    /// <param name="targetGroupId">Group Id to get</param>
    /// <returns>A list contains GroupMember</returns>
    public async Task<List<GroupMember>> GetGroupMemberList(object targetGroupId)
    {
        return await ApiPostRequestWithResponse<List<GroupMember>>("/get_group_member_list", new
        {
            group_id = targetGroupId,
            no_cache = false
        }) ?? [];
    }
    public async Task<bool> CheckUin(long targetGroupId, long targetUin)
    {
        return ((await GetGroupMember(targetGroupId, targetUin))?.GroupId ?? 0)  != 0;
    }
    private async Task<T?> ApiPostRequestWithResponse<T>(string subUrl, object? content = null)
    {
        try
        {
            HttpResponseMessage response = await HttpClient.PostAsync(HttpPostUrl + subUrl,
                content != null ?
                    new StringContent(
                        JsonSerializer.Serialize(content),
                        Encoding.UTF8,
                        "application/json"
                    ) : null
            );
            return response.IsSuccessStatusCode ? JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync()) : default;
        }
        catch (TaskCanceledException)
        {
            _httpApiLogger.Error("Task Canceled: ");
            return default;
        }
        catch (Exception ex)
        {
            PrintExceptionInfo(ex);
            return default;
        }
    }
    private async Task ApiPostRequestWithoutResponse(string subUrl, object? content = null)
    {
        try
        {
            _ = await HttpClient.PostAsync(HttpPostUrl + subUrl,
                content != null ?
                    new StringContent(
                        JsonSerializer.Serialize(content),
                        Encoding.UTF8,
                        "application/json"
                    ) : null
            );
        }
        catch (TaskCanceledException)
        {
            _httpApiLogger.Error("Task Canceled: ");
        }
        catch (Exception ex)
        {
            PrintExceptionInfo(ex);
        }
    }
    private void PrintExceptionInfo(Exception ex)
    {
        _httpApiLogger.Error("Error Occured, Error Information:");
        _httpApiLogger.Error(ex.Message);
        _httpApiLogger.Error(ex.StackTrace ?? "");
    }
}

[Serializable] public class GroupMember
{
    [JsonPropertyName("group_id")] public long GroupId { get; set; } = 0;
    [JsonPropertyName("user_id")] public long UserId { get; set; } = 0;
    [JsonPropertyName("nickname")] public string Nickname { get; set; } = "";
    [JsonPropertyName("card")] public string Card { get; set; } = "";
    [JsonPropertyName("sex")] public string Sex { get; set; } = "";
    [JsonPropertyName("age")] public int Age { get; set; } = 0;
    [JsonPropertyName("area")] public string Area { get; set; } = "";
    [JsonPropertyName("join_time")] public int JoinTime { get; set; } = 0;
    [JsonPropertyName("last_sent_time")] public int LastSentTime { get; set; } = 0;
    [JsonPropertyName("level")] public string Level { get; set; } = "";
    [JsonPropertyName("role")] public string Role { get; set; } = "member";
    [JsonPropertyName("unfriendly")] public bool Unfriendly { get; set; } = false;
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("title_expire_time")] public int TitleExpireTime { get; set; } = 0;
    [JsonPropertyName("card_changeable")] public bool CardChangeable { get; set; } = true;
}
