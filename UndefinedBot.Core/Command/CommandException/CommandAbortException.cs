namespace UndefinedBot.Core.Command.CommandException;

public class CommandAbortException(string? message = null) : Exception(message);