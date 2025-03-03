using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UndefinedBot.Core.Utils;

[assembly: InternalsVisibleTo("UndefinedBot.Net")]

namespace UndefinedBot.Core;

internal static class Shared
{
    //This is a shared logger factory for all classes, will be assigned before loading
    [AllowNull] public static ILoggerFactory LoggerFactory { get; set; }
}