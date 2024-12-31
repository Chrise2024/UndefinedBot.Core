using System.Text.Json;
using System.Text.Json.Nodes;

namespace UndefinedBot.Core.Utils;

internal static class FileIO
{
    private static readonly JsonSerializerOptions s_serializerOptio = new(){ WriteIndented = true };
    public static bool EnsurePath(string? tPath)
    {
        if (tPath == null)
        {
            return false;
        }

        if (Path.Exists(tPath))
        {
            return true;
        }

        Directory.CreateDirectory(tPath);
        return false;

    }

    public static bool EnsureFile(string tPath, string initData = "")
    {
        if (File.Exists(tPath))
        {
            return true;
        }

        EnsurePath(Path.GetDirectoryName(tPath));
        File.Create(tPath).Close();
        WriteFile(tPath, initData.Length != 0 ? initData : string.Empty);
        return false;
    }

    public static void SafeDeleteFile(string tPath)
    {
        try
        {
            if (File.Exists(tPath))
            {
                File.Delete(tPath);
            }
        }
        catch
        {
            // ignored
        }
    }

    public static void SafeDeletePath(string tPath)
    {
        try
        {
            if (Path.Exists(tPath))
            {
                Directory.Delete(tPath);
            }
        }
        catch
        {
            // ignored
        }
    }

    public static string ReadFile(string tPath)
    {
        try
        {
            if (File.Exists(tPath))
            {
                return File.ReadAllText(tPath);
            }
        }
        catch
        {
            // ignored
        }

        return "";
    }

    public static void WriteFile(string tPath, string content)
    {
        try
        {
            EnsureFile(tPath, content);
            File.WriteAllText(tPath, content);
        }
        catch
        {
            // ignored
        }
    }

    public static JsonNode? ReadAsJson(string tPath)
    {
        try
        {
            string content = ReadFile(tPath);
            if (content.Length != 0)
            {
                return JsonNode.Parse(content);
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    public static T? ReadAsJson<T>(string tPath)
    {
        try
        {
            string content = ReadFile(tPath);
            if (content.Length != 0)
            {
                return JsonSerializer.Deserialize<T>(content);
            }
        }
        catch
        {
            // ignored
        }

        return default;
    }

    public static void WriteAsJson<T>(string tPath, T content)
    {
        EnsureFile(tPath);
        WriteFile(tPath, JsonSerializer.Serialize(content, s_serializerOptio));
    }
}
