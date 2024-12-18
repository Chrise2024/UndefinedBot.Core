using UndefinedBot.Core.Adapter;

namespace Adapter.OneBot11;

public class OneBot11Adapter : BaseAdapter
{
    protected override string Name => "OneBot11Adapter";
    protected override string Platform => "QQ";
    protected override string Protocol => "OneBot11";
    private Task MainLoopInstance { get; set; }

    public OneBot11Adapter(AdapterConfigData adapterConfig) : base(adapterConfig)
    {
        HttpServer hs  = new(Logger,adapterConfig,SubmitCommandEvent);
        MainLoopInstance = hs.ExecuteAsync(new CancellationToken());
    }

    public override void HandleAdapterAction(string action, object paras)
    {
        //None
    }
    public override void HandleAdapterAction(DefaultActionType action, object paras)
    {
        //None
    }
}
