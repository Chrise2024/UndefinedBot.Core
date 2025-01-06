using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using UndefinedBot.Core.Command;
using UndefinedBot.Core.Command.CommandNodes;
using UndefinedBot.Core.Command.CommandResult;
using UndefinedBot.Core.Command.CommandSource;

namespace UndefinedBot.Core.Plugin;

internal static class CommandInvokeManager
{
    private static Dictionary<string, CommandInstance[]> CommandInstanceIndexByAdapter { get; set; } = [];
    public static async Task<CommandInvokeResult> InvokeCommand(CommandInvokeProperties invokeProperties,
        BaseCommandSource source)
    {
        if (!CommandInstanceIndexByAdapter.TryGetValue(invokeProperties.AdapterId, out var refCollection))
        {
            return CommandInvokeResult.NoCommandRelateToAdapter;
        }

        CommandInstance? targetCommand = Array
            .Find(
                refCollection,
                t => t.IsTargetCommand(invokeProperties)
            );
        if (targetCommand is null)
        {
            return CommandInvokeResult.NoSuchCommand;
        }

        if (targetCommand.IsReachRateLimit(invokeProperties))
        {
            return CommandInvokeResult.CommandRateLimited;
        }
        CommandContext ctx = new(targetCommand.Name, targetCommand.PluginId, invokeProperties);
        ctx.Logger.Info("Command Triggered");
        try
        {
            ICommandResult result = await targetCommand.Run(ctx, source, invokeProperties.Tokens);
            switch (result)
            {
                case CommandSuccess:
                    //ignore
                    break;
                case InvalidArgument iae:
                    ctx.Logger.Error(
                        $"Invalid argument: {iae.ErrorToken}, require {JsonSerializer.Serialize(iae.RequiredType)}");
                    break;
                case TooLessArgument tae:
                    ctx.Logger.Error(
                        $"To less arguments, require {JsonSerializer.Serialize(tae.RequiredType)}");
                    break;
                case PermissionDenied pde:
                    ctx.Logger.Error(
                        $"Not enough permission: {pde.CurrentPermission} at {pde.CurrentNode}, require {pde.RequiredPermission}");
                    break;
            }
        }
        catch (CommandAbortException)
        {
            ctx.Logger.Error($"Command Execute Aborted");
        }
        catch (CommandSyntaxException cse)
        {
            ctx.Logger.Error($"Node {cse.CurrentNode} Not Implemented");
        }
        catch (Exception ex)
        {
            ctx.Logger.Error(ex, "Command Failed");
        }

        return CommandInvokeResult.SuccessInvoke;
    } 
    public static void UpdateCommandInstances(IEnumerable<CommandInstance> ci)
    {
        CommandInstanceIndexByAdapter.Clear();
        GC.Collect();
        CommandInstanceIndexByAdapter = ci
            .GroupBy(
                i => i.TargetAdapterId,
                p => p
            )
            .ToDictionary(
                k => k.Key,
                v => v.ToArray()
            );
    }
}

public enum CommandInvokeResult
{
    SuccessInvoke = 0,
    NoCommandRelateToAdapter = 1,
    NoSuchCommand = 2,
    CommandRateLimited = 3,
}