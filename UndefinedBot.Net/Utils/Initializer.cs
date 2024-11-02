using System;
using System.Reflection;
using System.Text.Json.Serialization;
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

        private static readonly Logger s_initLogger = new("Program", "Initialize");
        internal static Dictionary<string, CommandInstanceSchematics> LoadPlugins()
        {
            if (Directory.Exists(s_pluginRoot))
            {
                Dictionary<string, CommandInstanceSchematics> CommandReference = [];
                string[] PluginFolders = Directory.GetDirectories(s_pluginRoot);
                foreach (string pf in PluginFolders)
                {
                    string[] Files = [.. Directory.GetFiles(pf).Where(name => name.EndsWith(".dll"))];
                    string PluginPropFile = Path.Join(pf, "plugin.json");
                    if (Files.Length > 0 && File.Exists(PluginPropFile))
                    {
                        JObject PluginPropertieJson = FileIO.ReadAsJSON(PluginPropFile);
                        if (PluginPropertieJson.IsValid(PluginPropJsonSchema))
                        {

                            PluginPropertieSchematics PluginPropertie = PluginPropertieJson.ToObject<PluginPropertieSchematics>();
                            foreach (CommandPropertieSchematics ep in PluginPropertie.Commands)
                            {
                                object? CInstance = InitCommand(Files[0], ep.EntryPoint);
                                if (CInstance != null)
                                {
                                    CommandReference[PluginPropertie.Name] = new CommandInstanceSchematics(PluginPropertie.Name, ep, CInstance);
                                }
                                else
                                {
                                    s_initLogger.Warn($"Command: <{ep.Name}> load failed, at plugin <{PluginPropertie.Name}>");
                                }
                            }
                        }
                        else
                        {
                            s_initLogger.Warn($"Plugin: <{pf}> load failed");
                        }
                    }
                    else
                    {
                        s_initLogger.Warn($"Plugin: <{pf}> load failed");
                    }
                }
                return CommandReference;
            }
            else
            {
                Directory.CreateDirectory(s_pluginRoot);
                return [];
            }
        }
        internal static object? InitCommand(string pluginDllPath,string entryPoint)
        {
            try
            {
                Assembly PluginAssembly = Assembly.LoadFrom(pluginDllPath);
                IEnumerable<Type> Types = PluginAssembly.GetTypes().Where(t => t.IsClass && t.Name == entryPoint);
                foreach (Type type in Types)
                {
                    var CInstance = Activator.CreateInstance(type);
                    if (CInstance != null)
                    {
                        MethodInfo? InitMethod = type.GetMethod("Init");
                        InitMethod?.Invoke(CInstance, null);
                        return CInstance;
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
                          ""commands"": {
                              ""type"": ""array"",
                              ""items"": {
                                  ""type"": ""object"",
                                  ""properties"": {
                                      ""name"": { ""type"": ""string"" },
                                      ""description"": { ""type"": ""string"" },
                                      ""short_description"": { ""type"": ""string"" },
                                      ""entry_point"": { ""type"": ""string"" }
                                  },
                                  ""required"": [""name"", ""description"", ""short_description"", ""entry_point""]
                              }
                          }
                      },
                    ""required"": [""name"", ""description"", ""commands""]
                  }"
            );
    }
    internal struct PluginPropertieSchematics
    {
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("commands")] public List<CommandPropertieSchematics> Commands;
    }
    internal struct CommandPropertieSchematics
    {
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("short_description")] public string ShortDescription;
        [JsonProperty("entry_point")] public string EntryPoint;
    }
    internal struct CommandInstanceSchematics(string plugin,CommandPropertieSchematics prop,object instance)
    {
        [JsonProperty("plugin")] public string plugin = plugin;
        [JsonProperty("properties")] public CommandPropertieSchematics Properties = prop;
        [Newtonsoft.Json.JsonIgnore]public object Instance = instance;
    }
}
