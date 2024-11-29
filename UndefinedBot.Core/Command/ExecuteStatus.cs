// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace UndefinedBot.Core.Command
{
    public enum ExecuteStatus
    {
        Success = 0,
        Fail = 1,
        PermissionDenied = 2,
        InvalidArgument = 3,
        NullArgument = 4,
        InvalidSyntax = 5,
    }
}
