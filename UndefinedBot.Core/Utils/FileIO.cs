using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UndefinedBot.Core.Utils
{
    internal abstract class FileIO
    {
        public static void EnsurePath(string? tPath)
        {
            if (tPath == null)
            {
                return;
            }
            if (!Path.Exists(tPath))
            {
                Directory.CreateDirectory(tPath);
            }
        }
        public static void EnsureFile(string tPath, string initData = "")
        {
            if (!File.Exists(tPath))
            {
                EnsurePath(Path.GetDirectoryName(tPath));
                File.Create(tPath).Close();
                WriteFile(tPath, initData.Length != 0 ? initData : string.Empty);
            }
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
        public static JObject ReadAsJson(string tPath)
        {
            try
            {
                string content = ReadFile(tPath);
                if (content.Length != 0)
                {
                    return JObject.Parse(content);
                }
            }
            catch
            {
                // ignored
            }

            return [];
        }
        public static T? ReadAsJson<T>(string tPath)
        {
            try
            {
                string content = ReadFile(tPath);
                if (content.Length != 0)
                {
                    return JsonConvert.DeserializeObject<T>(content);
                }
            }
            catch
            {
                // ignored
            }

            return new JObject().ToObject<T>();
        }
        public static void WriteAsJson<T>(string tPath, T content)
        {
            EnsureFile(tPath);
            WriteFile(tPath, JsonConvert.SerializeObject(content, Formatting.Indented));
        }
        public static void WriteAsJson(string tPath, JObject content)
        {
            EnsureFile(tPath);
            WriteFile(tPath, JsonConvert.SerializeObject(content, Formatting.Indented));
        }
    }
}
