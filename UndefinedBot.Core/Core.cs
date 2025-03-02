using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using UndefinedBot.Core.Utils;

[assembly: InternalsVisibleTo("UndefinedBot.Net")]

namespace UndefinedBot.Core;

internal static class Core
{
    public static readonly JsonNode RootConfig;

    static Core()
    {
        RootConfig = FileIO.ReadAsJson(Path.Join(Environment.CurrentDirectory, "appsettings.json")) ??
                     throw new FileNotFoundException();
    }
}

public abstract class Logging;

public sealed class ProgramEnvironmentIfo
{
    public static ProgramEnvironmentIfo Instance => new();
    public string ProgramRootPath => Environment.CurrentDirectory;
    public PlatformID PlatForm => Environment.OSVersion.Platform;
    public bool Is64Bit => Environment.Is64BitOperatingSystem;
}