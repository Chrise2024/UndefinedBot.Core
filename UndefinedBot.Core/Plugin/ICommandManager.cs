using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.Arguments;
using UndefinedBot.Core.Command.CommandSource;

namespace UndefinedBot.Core.Plugin;

internal interface ICommandManager : IDisposable
{
    void InvokeCommandAsync(
        CommandInformation information,
        BaseCommandSource source,
        ParsedToken[] tokens
    );
}