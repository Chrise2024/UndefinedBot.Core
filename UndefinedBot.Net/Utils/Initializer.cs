using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using UndefinedBot.Core.Utils;
using UndefinedBot.Core.Command;

namespace UndefinedBot.Net.Utils
{
    abstract internal class Initializer
    {
        private static readonly string s_pluginRoot = Path.Join(Program.GetProgramRoot(),"Plugins");

        private static readonly Logger s_initLogger = new("Initialize");
        static internal List<PluginProperty> LoadPlugins()
        {
            if (Directory.Exists(s_pluginRoot))
            {
                string[] pluginFolders = Directory.GetDirectories(s_pluginRoot);
                List<PluginProperty> pluginRef = [];
                foreach (string pf in pluginFolders)
                {
                    string pluginPropFile = Path.Join(pf, "plugin.json");
                    string pluginUcFile = Path.Join(pf, "UndefinedBot.Core.dll");
                    if (File.Exists(pluginPropFile))
                    {
                        JObject pluginPropertyJson = FileIO.ReadAsJson(pluginPropFile);
                        //Console.WriteLine(JsonConvert.SerializeObject(pluginPropertyJson));
                        if (pluginPropertyJson.IsValid(s_pluginPropJsonSchema))
                        {
                            PluginProperty pluginProperty = pluginPropertyJson.ToObject<PluginProperty>();
                            string entryFile = Path.Join(pf, pluginProperty.EntryFile);
                            if (File.Exists(entryFile))
                            {
                                FileIO.SafeDeleteFile(pluginUcFile);
                                string pluginCachePath = Path.Join(Program.GetProgramCache(), pluginProperty.Name);
                                FileIO.EnsurePath(pluginCachePath);
                                foreach (string cf in Directory.GetFiles(pluginCachePath))
                                {
                                    FileIO.SafeDeleteFile(cf);
                                }
                                object? pInstance = InitPlugin(entryFile, pluginProperty.EntryPoint, pluginProperty.Name);
                                if (pInstance != null)
                                {
                                    pluginProperty.Instance = pInstance;
                                    pluginRef.Add(pluginProperty);
                                }
                                else
                                {
                                    s_initLogger.Warn($"Plugin: <{pf}> load failed");
                                }
                            }
                            else
                            {
                                s_initLogger.Warn($"Plugin: <{pf}> EntryFile: <{entryFile}> Not Found");
                            }
                        }
                        else
                        {
                            s_initLogger.Warn($"Plugin: <{pf}> Invalid plugin.json");
                        }
                    }
                }
                //Console.WriteLine(JsonConvert.SerializeObject(PluginRef));
                return pluginRef;
            }
            else
            {
                Directory.CreateDirectory(s_pluginRoot);
                return [];
            }
        }
        public static Dictionary<string, CommandProperty> GetCommandReference()
        {
            string[] crfs = Directory.GetFiles(Path.Join(Program.GetProgramRoot(),"CommandReference"));
            Dictionary<string, CommandProperty> commandRef = [];
            foreach (string cf in crfs)
            {
                List<CommandProperty> cfi = FileIO.ReadAsJson<List<CommandProperty>>(cf) ?? [];
                cfi.ForEach((i) =>
                {
                    if (i.Name != null)
                    {
                        commandRef[i.Name] = i;
                        //CommandRef.Add(i.Name, i);
                    }
                });
            }
            return commandRef;
        }
        private static object? InitPlugin(string pluginDllPath,string entryPoint, string pluginName)
        {
            try
            {
                Assembly pluginAssembly = Assembly.LoadFrom(pluginDllPath);
                IEnumerable<Type> types = pluginAssembly.GetTypes().Where(t => t.IsClass && t.Name == entryPoint);
                return types.Select(type => Activator.CreateInstance(type, [pluginName])).OfType<object>().FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
        private static readonly JSchema s_pluginPropJsonSchema = JSchema.Parse(
                @"{
                      ""type"": ""object"",
                      ""properties"": {
                          ""name"": { ""type"": ""string"" },
                          ""description"": { ""type"": ""string"" },
                          ""entry_file"": { ""type"": ""string"" },
                          ""entry_point"": { ""type"": ""string"" },
                      },
                    ""required"": [""name"", ""description"",""entry_file"", ""entry_point""]
                  }"
            );
    }
    internal struct PluginProperty
    {
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("entry_file")] public string EntryFile;
        [JsonProperty("entry_point")] public string EntryPoint;
        [JsonIgnore] public object? Instance;
    }
}
