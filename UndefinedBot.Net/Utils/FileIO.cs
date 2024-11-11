using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UndefinedBot.Net.Utils
{
    abstract internal class FileIO
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
            if (File.Exists(tPath))
            {
                return File.ReadAllText(tPath);
            }
            else
            {
                return "";
            }
        }
        public static void WriteFile(string tPath, string content)
        {
            EnsureFile(tPath, content);
            File.WriteAllText(tPath, content);
        }
        public static JObject ReadAsJson(string tPath)
        {
            string content = ReadFile(tPath);
            if (content.Length != 0)
            {
                return JObject.Parse(content);
            }
            else
            {
                return [];
            }
        }
        public static T? ReadAsJson<T>(string tPath)
        {
            string content = ReadFile(tPath);
            return content.Length != 0 ? JsonConvert.DeserializeObject<T>(content) : default;
        }
        public static JArray ReadAsJArray(string tPath)
        {

            return JArray.Parse(ReadFile(tPath));
        }
        public static void WriteAsJson<T>(string tPath, T content)
        {
            EnsureFile(tPath);
            WriteFile(tPath, JsonConvert.SerializeObject(content, Formatting.Indented));
        }
        public static void WriteAsJArray(string tPath, JArray content)
        {
            EnsureFile(tPath);
            WriteFile(tPath, JsonConvert.SerializeObject(content, Formatting.Indented));
        }
    }
}
