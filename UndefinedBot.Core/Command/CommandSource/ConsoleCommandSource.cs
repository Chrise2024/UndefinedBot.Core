// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace UndefinedBot.Core.Command.CommandSource;

internal sealed class ConsoleCommandSource : BaseCommandSource
{
    public override CommandSourceAuthority Authority { get; protected set; } = CommandSourceAuthority.Console;

    public override bool HasAuthorityLevel(CommandSourceAuthority authorityLevel)
    {
        return true;
    }

    public override bool IsFrom(CommandSourceType sourceType)
    {
        return sourceType == CommandSourceType.Console;
    }
}