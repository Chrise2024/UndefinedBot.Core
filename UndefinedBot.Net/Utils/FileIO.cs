using System;
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
                return;
            }
            else
            {
                return;
            }
        }
        public static void EnsureFile(string tPath, string initData = "")
        {
            if (!File.Exists(tPath))
            {
                EnsurePath(Path.GetDirectoryName(tPath));
                File.Create(tPath).Close();
                if (initData.Length != 0)
                {
                    WriteFile(tPath, initData);
                }
                else
                {
                    WriteFile(tPath, string.Empty);
                }
                return;
            }
            else
            {
                return;
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
            catch { }
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
            catch { }
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
            return;
        }
        public static JObject ReadAsJSON(string tPath)
        {
            string Content = ReadFile(tPath);
            if (Content.Length != 0)
            {
                return JObject.Parse(Content);
            }
            else
            {
                return [];
            }
        }
        public static T ReadAsJSON<T>(string tPath)
        {
            string Content = ReadFile(tPath);
            if (Content.Length != 0)
            {
                return JsonConvert.DeserializeObject<T>(Content);
            }
            else
            {
                return new JObject().ToObject<T>();
            }
        }
        public static JArray ReadAsJArray(string tPath)
        {

            return JArray.Parse(ReadFile(tPath));
        }
        public static void WriteAsJSON<T>(string tPath, T Content)
        {
            EnsureFile(tPath);
            WriteFile(tPath, JsonConvert.SerializeObject(Content, Formatting.Indented));
            return;
        }
        public static void WriteAsJSON(string tPath, JObject Content)
        {
            EnsureFile(tPath);
            WriteFile(tPath, JsonConvert.SerializeObject(Content, Formatting.Indented));
            return;
        }
        public static void WriteAsJArray(string tPath, JArray Content)
        {
            EnsureFile(tPath);
            WriteFile(tPath, JsonConvert.SerializeObject(Content, Formatting.Indented));
            return;
        }
    }
}
