using UndefinedBot.Core.Adapter;

namespace Adapter.OneBot11;

public class OneBot11Adapter : BaseAdapter
{
    public override string Id => "OneBot11Adapter";
    public override string Name => "OneBot11Adapter";
    public override string Platform => "QQ";
    public override string Protocol => "OneBot11";
    private Task MainLoopInstance { get; set; }

    public OneBot11Adapter(AdapterConfigData adapterConfig) : base(adapterConfig)
    {
        HttpServer hs  = new(Logger,adapterConfig,SubmitCommandEvent);
        MainLoopInstance = hs.ExecuteAsync(new CancellationToken());
    }

    public override void HandleCustomAction(string action, object paras)
    {
        //None
    }
    public override void HandleDefaultAction(DefaultActionType action, object paras)
    {
        //None
    }
}
