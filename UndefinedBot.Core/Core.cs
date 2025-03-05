using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UndefinedBot.Core.Utils;

[assembly: InternalsVisibleTo("UndefinedBot.Net")]

namespace UndefinedBot.Core;

internal static class Shared
{
    //This is a shared logger factory for all classes, will be assigned before loading
    [AllowNull] public static ILoggerFactory LoggerFactory { get; set; }
    [AllowNull] public static IReadonlyConfig RootConfig { get; set; }
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class AdapterAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
public sealed class PluginAttribute : Attribute;