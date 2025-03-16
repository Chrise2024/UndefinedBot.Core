// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace UndefinedBot.Core.Message;

internal sealed class ConsoleMessageSource : BaseMessageSource
{
    public override MessageSourceAuthority Authority { get; protected set; } = MessageSourceAuthority.Console;

    public override bool HasAuthorityLevel(MessageSourceAuthority authorityLevel)
    {
        return true;
    }

    public override bool IsFrom(MessageSourceType sourceType)
    {
        return sourceType == MessageSourceType.Console;
    }
}