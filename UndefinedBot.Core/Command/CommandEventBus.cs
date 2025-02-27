using System.Threading.Channels;
using UndefinedBot.Core.Command.CommandSource;
using UndefinedBot.Core.Plugin;

namespace UndefinedBot.Core.Command;

internal static class CommandEventBus
{
    private static readonly Channel<CommandEventWrapper> _commandEventChannel = Channel.CreateBounded<CommandEventWrapper>(new BoundedChannelOptions(128)
    {
        FullMode = BoundedChannelFullMode.DropOldest,
        SingleReader = true
    });
    public static async Task SendCommandEventAsync(CommandInformation basicInformation, BaseCommandSource source)
    {
        CommandEventWrapper wrapper = new(basicInformation, source);
        await _commandEventChannel.Writer.WriteAsync(wrapper);
    }
    public static async Task<CommandEventWrapper> ReadCommandEventAsync(CancellationToken token)
    {
        return await _commandEventChannel.Reader.ReadAsync(token);
    }
}

internal readonly struct CommandEventWrapper(CommandInformation basicInformation, BaseCommandSource source)
{
    public readonly CommandInformation BasicInformation = basicInformation;
    public readonly BaseCommandSource Source = source;
}