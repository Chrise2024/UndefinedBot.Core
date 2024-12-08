using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UndefinedBot.Core.Utils;

public enum ImageSendType
{
    LocalFile = 0,
    Url = 1,
    Base64 = 2
}

public enum ImageSubType
{
    Normal = 0, //正常图片
    Emoji = 1, //表情包
    Hot = 2, //热图
    Battle = 3, //斗图
}

public class MsgBuilder
{
    private MsgBuilder()
    {
    }

    public static MsgBuilder GetInstance()
    {
        return new MsgBuilder();
    }

    private readonly List<JObject> _msgChain = [];

    public MsgBuilder Text(string text)
    {
        _msgChain.Add(new JObject() { { "type", "text" }, { "data", new JObject() { { "text", text } } }, });
        return this;
    }

    public MsgBuilder QFace(string face)
    {
        _msgChain.Add(new JObject() { { "type", "text" }, { "data", new JObject() { { "id", face } } }, });
        return this;
    }

    public MsgBuilder Image(string imageContent, ImageSendType sendType = ImageSendType.LocalFile,
        ImageSubType subtype = ImageSubType.Normal)
    {
        string urlPrefix = sendType switch
        {
            ImageSendType.LocalFile => "file:///",
            ImageSendType.Base64 => "base64://",
            _ => ""
        };
        _msgChain.Add(new JObject()
        {
            { "type", "image" },
            { "data", new JObject() { { "file", $"{urlPrefix}{imageContent}" }, { "subType", (int)subtype } } },
        });
        return this;
    }

    public MsgBuilder At(long atUin)
    {
        _msgChain.Add(new JObject() { { "type", "at" }, { "data", new JObject() { { "qq", atUin } } }, });
        return this;
    }

    public MsgBuilder Reply(int msgId)
    {
        _msgChain.Add(new JObject() { { "type", "reply" }, { "data", new JObject() { { "id", $"{msgId}" } } }, });
        return this;
    }

    public List<JObject> Build()
    {
        return _msgChain;
    }
}

public class ForwardBuilder
{
    private ForwardBuilder()
    {
    }

    private readonly List<ForwardNode> _forwardNodes = [];
    private readonly List<ForwardSummaryNewsEntity> _forwardNews = [];
    private string? _prompt;

    public static ForwardBuilder GetInstance()
    {
        return new ForwardBuilder();
    }

    public ForwardNode AddNode<T>(string senderName, T senderUin)
    {
        ForwardNode node = new(senderName, $"{senderUin}", this);
        _forwardNodes.Add(node);
        return node;
    }

    public ForwardBuilder CustomPrompt(string prompt)
    {
        _prompt = prompt;
        return this;
    }
    public ForwardBuilder CustomNews(IEnumerable<string> newsText)
    {
        _forwardNews.AddRange(newsText.Select(item => new ForwardSummaryNewsEntity(item)));
        return this;
    }

    public ForwardSummaryData Finish()
    {
        return new ForwardSummaryData(_forwardNodes,_forwardNews.Count == 0 ? null : _forwardNews,_prompt);
    }
}

public class ForwardNode(string name, string uin, ForwardBuilder instance)
{
    [JsonProperty("type")] public readonly string Type = "node";
    [JsonProperty("data")] public readonly ForwardNodeData Data = new(name, uin, []);
    [JsonIgnore] private readonly ForwardBuilder _parentInstance = instance;

    public ForwardNode Text(string text)
    {
        Data.Content.Add(new JObject() { { "type", "text" }, { "data", new JObject() { { "text", text } } }, });
        return this;
    }

    public ForwardNode QFace(string face)
    {
        Data.Content.Add(new JObject() { { "type", "text" }, { "data", new JObject() { { "id", face } } }, });
        return this;
    }

    public ForwardNode Image(string imageContent, ImageSendType sendType = ImageSendType.LocalFile,
        ImageSubType subtype = ImageSubType.Normal)
    {
        string urlPrefix = sendType switch
        {
            ImageSendType.LocalFile => "file:///",
            ImageSendType.Base64 => "base64://",
            _ => ""
        };
        Data.Content.Add(new JObject()
        {
            { "type", "image" },
            { "data", new JObject() { { "file", $"{urlPrefix}{imageContent}" }, { "subType", (int)subtype } } },
        });
        return this;
    }

    public ForwardNode At(long atUin)
    {
        Data.Content.Add(new JObject() { { "type", "at" }, { "data", new JObject() { { "qq", atUin } } }, });
        return this;
    }

    public ForwardNode Reply(int msgId)
    {
        Data.Content.Add(new JObject() { { "type", "reply" }, { "data", new JObject() { { "id", $"{msgId}" } } }, });
        return this;
    }

    public ForwardBuilder Build()
    {
        return _parentInstance;
    }
}
public struct ForwardSummaryData(List<ForwardNode> content,
    IEnumerable<ForwardSummaryNewsEntity>? news = null,
    string? prompt = null
    )
{
    [JsonProperty("message")] public List<ForwardNode> Message = content;
    [JsonProperty("news")] public List<ForwardSummaryNewsEntity>? News = news?.ToList();
    [JsonProperty("prompt")] public string? Prompt = prompt;
}

public struct ForwardSummaryNewsEntity(string text)
{
    [JsonProperty("text")] public readonly string Text = text;
}
public struct ForwardNodeData(string name, string uin, List<JObject> content)
{
    [JsonProperty("name")] public string Name = name;
    [JsonProperty("uin")] public string Uin = uin;
    [JsonProperty("content")] public List<JObject> Content = content;
}
