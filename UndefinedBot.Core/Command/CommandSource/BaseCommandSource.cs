﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace UndefinedBot.Core.Command.CommandSource;

public abstract class BaseCommandSource
{
    public virtual string UserId { get; protected set; } = "";
    public virtual string UserName { get; protected set; } = "";
    public virtual string UserCard { get; protected set; } = "";
    public virtual string GroupId { get; protected set; } = "";
    public virtual CommandSourceAuthority Authority { get; protected set; } = CommandSourceAuthority.InvalidSource;
    public abstract bool HasAuthorityLevel(CommandSourceAuthority authorityLevel);
    public abstract bool IsFrom(CommandSourceType sourceType);
    internal CommandAttribFlags CurrentCommandAttrib { get; private set; } = CommandInstance.DefaultCommandAttrib;

    internal void SetCurrentCommandAttrib(CommandAttribFlags attr)
    {
        CurrentCommandAttrib = attr;
    }
}

public enum CommandSourceType
{
    User = 0,
    Console = 1,
    Redirect = 2
}

public enum CommandSourceAuthority
{
    InvalidSource = 0,
    User = 1,
    PowerUser = 2,
    Admin = 3,
    Operator = 4,
    Console = 5
}