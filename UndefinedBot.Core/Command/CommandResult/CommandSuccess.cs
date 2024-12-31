// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace UndefinedBot.Core.Command.CommandResult;

public sealed class CommandSuccess : ICommandResult
{
    public ExecuteStatus Status => ExecuteStatus.Success;
}
