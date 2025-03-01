using UndefinedBot.Core.Command.CommandSource;

namespace UndefinedBot.Core.Command;

internal readonly struct CommandEventWrapper(CommandInformation basicInformation, BaseCommandSource source)
{
    public readonly CommandInformation BasicInformation = basicInformation;
    public readonly BaseCommandSource Source = source;
}