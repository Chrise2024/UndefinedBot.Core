# UndefinedBot.Core

The SDK of UndefinedBot.

## Plugin Meta

In `plugin.json`

```json
{
  "name": "Help",
  "description": "帮助",
  "entry_file": "Command.Help"
}
```

- `name` The name of plugin
- `description` The description of plugin
- `entry_file` The binary assembly of plugin(Without Suffix)

## Plugin Develop

Plugins are developing with `UndefinedBot.Core` on Nuget or as git submodule.

Plugin's main class must not be abstract or static and must implement interface `UndefinedBot.Core.Registry.IPluginInitializer`.

You can register commands in `Initialize` method.

Example:

```csharp
public class TestCommand : IPluginInitializer
{
    public void Initialize(UndefinedApi undefinedApi)
    {
        undefinedApi.RegisterCommand("test")
            .Description("指令帮助文档")
            .ShortDescription("帮助")
            .Usage("{0}test [xxx]")
            .Example("{0}test 114514")
            .Execute(async (ctx) => { })
            .Then(new VariableNode("var1", new IntegerArgument()));
    }
}
```

## Plugins Loading

## What happened in loading plugins

- Main program start

- `Initializer.LoadPlugins()` is called

- `LoadPlugins()` will read `plugin.json` in plugin folder in `./Plugins` and get plugin's meta data

- load plugin assembly and invoke `Initialize` method

- commands in plugin will be registered,creating command instance and generate command reference

- return plugin reference and command instance to main program

- `Initializer.GetCommandReference()` is called

- `GetCommandReference()` will collect command reference and store them in program directory

- Start to Listen to http port

- Bot Launched
