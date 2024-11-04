using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using UndefinedBot.Core.Utils;
using UndefinedBot.Net;

namespace UndefinedBot.Core.Command
{
    internal class Initializer
    {
        private static readonly string s_pluginRoot = Path.Join(Program.GetProgramRoot(),"Plugins");

        private static readonly Logger s_initLogger = new("Initialize");
        internal static List<PluginPropertieSchematics> LoadPlugins()
        {
            if (Directory.Exists(s_pluginRoot))
            {
                string[] PluginFolders = Directory.GetDirectories(s_pluginRoot);
                List<PluginPropertieSchematics> PluginRef = [];
                foreach (string pf in PluginFolders)
                {
                    string[] Files = [.. Directory.GetFiles(pf).Where(name => name.EndsWith(".dll"))];
                    string PluginPropFile = Path.Join(pf, "plugin.json");
                    if (Files.Length > 0 && File.Exists(PluginPropFile))
                    {
                        JObject PluginPropertyJson = FileIO.ReadAsJSON(PluginPropFile);
                        if (PluginPropertyJson.IsValid(PluginPropJsonSchema))
                        {

                            PluginPropertieSchematics PluginProperty = PluginPropertyJson.ToObject<PluginPropertieSchematics>();
                            FileIO.EnsurePath(Path.Join(Program.GetProgramCahce(), PluginProperty.Name));
                            object? PInstance = InitPlugin(Files[0], PluginProperty.EntryPoint, PluginProperty.Name);
                            if (PInstance != null)
                            {
                                PluginProperty.Instance = PInstance;
                                PluginRef.Add(PluginProperty);
                            }
                            else
                            {
                                s_initLogger.Warn("Program", $"Plugin: <{pf}> load failed");
                            }
                        }
                        else
                        {
                            s_initLogger.Warn("Program", $"Plugin: <{pf}> load failed");
                        }
                    }
                    else
                    {
                        s_initLogger.Warn("Program", $"Plugin: <{pf}> load failed");
                    }
                }
                //Console.WriteLine(JsonConvert.SerializeObject(PluginRef));
                return PluginRef;
            }
            else
            {
                Directory.CreateDirectory(s_pluginRoot);
                return [];
            }
        }
        public static Dictionary<string,CommandInstance> GetCommandReference()
        {
            string[] crfs = Directory.GetFiles(Path.Join(Program.GetProgramRoot(),"CommandReference"));
            Dictionary<string, CommandInstance> CommandRef = [];
            foreach (string cf in crfs)
            {
                CommandInstance cfi = FileIO.ReadAsJSON<CommandInstance>(cf);
                if (cfi.Name != null)
                {
                    CommandRef.Add(cfi.Name, cfi);
                }
            }
            return CommandRef;
        }
        private static object? InitPlugin(string pluginDllPath,string entryPoint, string pluginName)
        {
            try
            {
                Assembly PluginAssembly = Assembly.LoadFrom(pluginDllPath);
                IEnumerable<Type> Types = PluginAssembly.GetTypes().Where(t => t.IsClass && t.Name == entryPoint);
                foreach (Type type in Types)
                {
                    var PInstance = Activator.CreateInstance(type, [pluginName]);
                    if (PInstance != null)
                    {
                        return PInstance;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
        internal static JSchema PluginPropJsonSchema = JSchema.Parse(
                @"{
                      ""type"": ""object"",
                      ""properties"": {
                          ""name"": { ""type"": ""string"" },
                          ""description"": { ""type"": ""string"" },
                          ""entry_point"": { ""type"": ""string"" },
                      },
                    ""required"": [""name"", ""description"", ""entry_point""]
                  }"
            );
    }
    internal struct PluginPropertieSchematics
    {
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("entry_point")] public string EntryPoint;
        [JsonIgnore] public object? Instance;
    }
    internal struct CommandPropertieSchematics
    {
        [JsonProperty("name")] public string Name;
        [JsonProperty("alias")] public List<string> Alias;
        [JsonProperty("description")] public string? Description;
        [JsonProperty("short_description")] public string? ShortDescription;
        [JsonProperty("example")] public string? Example;
    }
}
