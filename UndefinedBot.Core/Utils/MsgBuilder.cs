using System.Text.Json;
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

public class MsgBuilder
{
    private readonly JsonNode _textElementBase = JsonNode.Parse("""{"type": "text", "data": {"text": ""}}""")!;
    private readonly JsonNode _faceElementBase = JsonNode.Parse("""{"type": "face", "data": {"id": ""}}""")!;
    private readonly JsonNode _imageElementBase = JsonNode.Parse("""{"type": "image", "data": {"file": "", "subType": ""}}""")!;
    private readonly JsonNode _atElementBase = JsonNode.Parse("""{"type": "at", "data": {"qq": ""}}""")!;
    private readonly JsonNode _replyElementBase = JsonNode.Parse("""{"type": "reply", "data": {"id": ""}}""")!;
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
    public MsgBuilder At(QUin atUin)
    {
        JsonNode temp = _atElementBase.DeepClone();
        temp["data"]!["at"] = $"{atUin.Uin}";
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
    private readonly JsonNode _textElementBase = JsonNode.Parse("""{"type": "text", "data": {"text": ""}}""")!;
    private readonly JsonNode _faceElementBase = JsonNode.Parse("""{"type": "face", "data": {"id": ""}}""")!;
    private readonly JsonNode _imageElementBase = JsonNode.Parse("""{"type": "image", "data": {"file": "", "subType": ""}}""")!;
    private readonly JsonNode _atElementBase = JsonNode.Parse("""{"type": "at", "data": {"qq": ""}}""")!;
    private readonly JsonNode _replyElementBase = JsonNode.Parse("""{"type": "reply", "data": {"id": ""}}""")!;
    [JsonPropertyName("type")] public readonly string Type = "node";
    [JsonPropertyName("data")] private readonly ForwardNodeData _data = new(name, uin, []);
    [JsonIgnore] private readonly ForwardBuilder _parentInstance = instance;

    public ForwardNode Text(string text)
    {
        JsonNode temp = _textElementBase.DeepClone();
        temp["data"]!["text"] = text;
        _data.Content.Add(temp);
        return this;
    }

    public ForwardNode QFace(string face)
    {
        JsonNode temp = _faceElementBase.DeepClone();
        temp["data"]!["face"] = face;
        _data.Content.Add(temp);
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
        JsonNode temp = _imageElementBase.DeepClone();
        temp["data"]!["file"] = $"{urlPrefix}{imageContent}";
        temp["data"]!["subType"] = $"{(int)subtype}";
        _data.Content.Add(temp);
        return this;
    }

    public ForwardNode At(long atUin)
    {
        JsonNode temp = _atElementBase.DeepClone();
        temp["data"]!["at"] = $"{atUin}";
        return this;
    }

    public ForwardNode Reply(int msgId)
    {
        JsonNode temp = _replyElementBase.DeepClone();
        temp["data"]!["id"] = $"{msgId}";
        _data.Content.Add(temp);
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
    [JsonPropertyName("message")] public List<ForwardNode> Message = content;
    [JsonPropertyName("news")] public List<ForwardSummaryNewsEntity>? News = news?.ToList();
    [JsonPropertyName("prompt")] public string? Prompt = prompt;
}

public struct ForwardSummaryNewsEntity(string text)
{
    [JsonPropertyName("text")] public readonly string Text = text;
}
public readonly struct ForwardNodeData(string name, string uin, List<JsonNode> content)
{
    [JsonPropertyName("name")] public readonly string Name = name;
    [JsonPropertyName("uin")] public readonly string Uin = uin;
    [JsonPropertyName("content")] public readonly List<JsonNode> Content = content;
}
