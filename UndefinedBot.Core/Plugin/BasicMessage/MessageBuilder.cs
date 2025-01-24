namespace UndefinedBot.Core.BasicMessage;

public sealed class MessageBuilder
{
    private readonly List<IMessageNode> _msgChain = [];

    public MessageBuilder Text(string text)
    {
        _msgChain.Add(new TextMessageNode{Text = text});
        return this;
    }

    public MessageBuilder QFace(string face)
    {
        throw new NotImplementedException(nameof(QFace));
        //_msgChain.Add(temp);
        return this;
    }

    public MessageBuilder Image(string imageUrl)
    {
        _msgChain.Add(new ImageMessageNode{Url = imageUrl});
        return this;
    }

    public MessageBuilder At(string atUid)
    {
        _msgChain.Add(new AtMessageNode{UserId = atUid});
        return this;
    }

    public MessageBuilder Reply(string rid)
    {
        _msgChain.Add(new ReplyMessageNode{ReplyToId = rid});
        return this;
    }

    public List<IMessageNode> Build()
    {
        return _msgChain;
    }
}