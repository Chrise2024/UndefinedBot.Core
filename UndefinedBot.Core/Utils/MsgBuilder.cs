using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using UndefinedBot.Core.Command.Arguments.ArgumentType;

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

public sealed class MsgBuilder
{
    private static readonly JsonNode _textElementBase = JsonNode.Parse("""{"type": "text", "data": {"text": ""}}""")!;
    private static readonly JsonNode _faceElementBase = JsonNode.Parse("""{"type": "face", "data": {"id": ""}}""")!;

    private static readonly JsonNode _imageElementBase =
        JsonNode.Parse("""{"type": "image", "data": {"file": "", "subType": ""}}""")!;

    private static readonly JsonNode _atElementBase = JsonNode.Parse("""{"type": "at", "data": {"qq": ""}}""")!;
    private static readonly JsonNode _replyElementBase = JsonNode.Parse("""{"type": "reply", "data": {"id": ""}}""")!;

    private MsgBuilder()
    {
    }

    public static MsgBuilder GetInstance()
    {
        return new MsgBuilder();
    }

    private readonly List<JsonNode> _msgChain = [];

    public MsgBuilder Text(string text)
    {
        JsonNode temp = _textElementBase.DeepClone();
        temp["data"]!["text"] = text;
        _msgChain.Add(temp);
        return this;
    }

    public MsgBuilder QFace(string face)
    {
        JsonNode temp = _faceElementBase.DeepClone();
        temp["data"]!["face"] = face;
        _msgChain.Add(temp);
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
        JsonNode temp = _imageElementBase.DeepClone();
        temp["data"]!["file"] = $"{urlPrefix}{imageContent}";
        temp["data"]!["subType"] = $"{(int)subtype}";
        _msgChain.Add(temp);
        return this;
    }

    public MsgBuilder At(long atUin)
    {
        JsonNode temp = _atElementBase.DeepClone();
        temp["data"]!["at"] = $"{atUin}";
        _msgChain.Add(temp);
        return this;
    }

    public MsgBuilder At(string atUin)
    {
        JsonNode temp = _atElementBase.DeepClone();
        temp["data"]!["at"] = $"{atUin}";
        _msgChain.Add(temp);
        return this;
    }

    public MsgBuilder Reply(int msgId)
    {
        JsonNode temp = _replyElementBase.DeepClone();
        temp["data"]!["id"] = $"{msgId}";
        _msgChain.Add(temp);
        return this;
    }

    public List<JsonNode> Build()
    {
        return _msgChain;
    }
}

public sealed class ForwardMessageBuilder
{
    private ForwardMessageBuilder()
    {
    }

    private readonly List<ForwardMessageNode> _forwardNodes = [];
    private readonly List<ForwardSummaryNewsEntity> _forwardNews = [];
    private string? _prompt;

    public static ForwardMessageBuilder GetInstance()
    {
        return new ForwardMessageBuilder();
    }

    public ForwardMessageNode AddNode<T>(string senderName, T senderUin)
    {
        ForwardMessageNode messageNode = new(senderName, $"{senderUin}", this);
        _forwardNodes.Add(messageNode);
        return messageNode;
    }

    public ForwardMessageBuilder CustomPrompt(string prompt)
    {
        _prompt = prompt;
        return this;
    }

    public ForwardMessageBuilder CustomNews(List<string> newsText)
    {
        _forwardNews.AddRange(newsText.Select(item => new ForwardSummaryNewsEntity(item)));
        return this;
    }

    public ForwardPropertiesData Finish()
    {
        return new ForwardPropertiesData(_forwardNodes, _forwardNews.Count == 0 ? null : _forwardNews, _prompt);
    }
}

[Serializable]
public sealed class ForwardMessageNode(string name, string uin, ForwardMessageBuilder instance)
{
    [JsonIgnore]
    private static readonly JsonNode _textElementBase = JsonNode.Parse("""{"type": "text", "data": {"text": ""}}""")!;

    [JsonIgnore]
    private static readonly JsonNode _faceElementBase = JsonNode.Parse("""{"type": "face", "data": {"id": ""}}""")!;

    [JsonIgnore] private static readonly JsonNode _imageElementBase =
        JsonNode.Parse("""{"type": "image", "data": {"file": "", "subType": ""}}""")!;

    [JsonIgnore]
    private static readonly JsonNode _atElementBase = JsonNode.Parse("""{"type": "at", "data": {"qq": ""}}""")!;

    [JsonIgnore]
    private static readonly JsonNode _replyElementBase = JsonNode.Parse("""{"type": "reply", "data": {"id": ""}}""")!;

    [JsonPropertyName("type")] public string Type => "node";
    [JsonPropertyName("data")] public ForwardNodeData Data => new(name, uin, []);
    [JsonIgnore] private readonly ForwardMessageBuilder _parentInstance = instance;

    public ForwardMessageNode Text(string text)
    {
        JsonNode temp = _textElementBase.DeepClone();
        temp["data"]!["text"] = text;
        Data.Content.Add(temp);
        return this;
    }

    public ForwardMessageNode QFace(string face)
    {
        JsonNode temp = _faceElementBase.DeepClone();
        temp["data"]!["face"] = face;
        Data.Content.Add(temp);
        return this;
    }

    public ForwardMessageNode Image(string imageContent, ImageSendType sendType = ImageSendType.LocalFile,
        ImageSubType subtype = ImageSubType.Normal)
    {
        string urlPrefix = sendType switch
        {
            ImageSendType.LocalFile => "file:///",
            ImageSendType.Base64 => "base64://",
            _ => ""
        };
        JsonNode temp = _imageElementBase.DeepClone();
        temp["data"]!["file"] = $"{urlPrefix}{imageContent}";
        temp["data"]!["subType"] = $"{(int)subtype}";
        Data.Content.Add(temp);
        return this;
    }

    public ForwardMessageNode At(long atUin)
    {
        JsonNode temp = _atElementBase.DeepClone();
        temp["data"]!["at"] = $"{atUin}";
        return this;
    }

    public ForwardMessageNode Reply(int msgId)
    {
        JsonNode temp = _replyElementBase.DeepClone();
        temp["data"]!["id"] = $"{msgId}";
        Data.Content.Add(temp);
        return this;
    }

    public ForwardMessageBuilder Build()
    {
        return _parentInstance;
    }
}

public sealed class ForwardPropertiesData(
    List<ForwardMessageNode> content,
    List<ForwardSummaryNewsEntity>? news = null,
    string? prompt = null
)
{
    [JsonPropertyName("message")] public List<ForwardMessageNode> Message => content;
    [JsonPropertyName("news")] public List<ForwardSummaryNewsEntity>? News => news;
    [JsonPropertyName("prompt")] public string? Prompt => prompt;
}

public sealed class ForwardSummaryNewsEntity(string text)
{
    [JsonPropertyName("text")] public string Text => text;
}

public sealed class ForwardNodeData(string name, string uin, List<JsonNode> content)
{
    [JsonPropertyName("name")] public string Name => name;
    [JsonPropertyName("uin")] public string Uin => uin;
    [JsonPropertyName("content")] public List<JsonNode> Content => content;
}