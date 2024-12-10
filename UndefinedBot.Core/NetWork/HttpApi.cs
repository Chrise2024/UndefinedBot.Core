using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;

namespace UndefinedBot.Core.NetWork;

public class HttpApi([StringSyntax("Uri")]string httpPostUrl)
{
    private readonly HttpClient _httpClient = new()
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
    /// <param name="forwardSummaryData">Onebot11 Forward MessageChain</param>
    public async Task SendGroupForward(object targetGroupId,ForwardSummaryData forwardSummaryData)
    {
        //JsonNode reqJson = JsonNode.FromObject(forwardSummaryData);
        JsonNode reqJson = JsonSerializer.SerializeToNode(forwardSummaryData)!;
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
    public async Task<MsgBody> GetMsg(object msgId)
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
    public async Task<GroupMember> GetGroupMember(object targetGroupId, object targetUin)
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
        return ((await GetGroupMember(targetGroupId, targetUin)).GroupId ?? 0) != 0;
    }
    private async Task<T?> ApiPostRequestWithResponse<T>(string subUrl, object? content = null)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(httpPostUrl + subUrl,
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
            _ = await _httpClient.PostAsync(httpPostUrl + subUrl,
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
public struct GroupMember
{
    [JsonPropertyName("group_id")] public long? GroupId;
    [JsonPropertyName("user_id")] public long? UserId;
    [JsonPropertyName("nickname")] public string? Nickname;
    [JsonPropertyName("card")] public string? Card;
    [JsonPropertyName("sex")] public string? Sex;
    [JsonPropertyName("age")] public int? Age;
    [JsonPropertyName("area")] public string? Area;
    [JsonPropertyName("join_time")] public int? JoinTime;
    [JsonPropertyName("last_sent_time")] public int? LastSentTime;
    [JsonPropertyName("level")] public string? Level;
    [JsonPropertyName("role")] public string? Role;
    [JsonPropertyName("unfriendly")] public bool? Unfriendly;
    [JsonPropertyName("title")] public string? Title;
    [JsonPropertyName("title_expire_time")] public int? TitleExpireTime;
    [JsonPropertyName("card_changeable")] public bool? CardChangeable;
}
