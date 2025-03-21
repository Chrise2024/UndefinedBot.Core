﻿# UndefinedBot.Core

The SDK of UndefinedBot.

Use `git submodule` to refer to project.

## Plugin Config

In `plugin.json`

```json5
{
    "Description": "帮助",
    "EntryFile": "Command.Help",
    "GroupId" : []
    //other content
}
```

- `Description` The description of plugin
- `EntryFile` The binary assembly of plugin(Without Suffix)
- `GroupIds` The groups plugin will be active in
- You can write other content in this file. The custom content will be provided

## Plugin Develop

Plugins are developing with `UndefinedBot.Core` on Nuget or as git submodule.

Plugin's main class must not be abstract or static and

- Must extend class `UndefinedBot.Core.Plugin.BasePlugin` and
  override it's abstract fields and `Initialize` method and have a constructor with single param `PluginDependencyCollection`.

- Mark class as `partial` and apply `PluginAttribute` to the class and do nothing on class declaration.Source Generator will implement the class.

Use `RegisterCommand` method extends form BasePlugin to register command

> Suggest to register commands in `Initialize` method, rather than constructor.

Example:

Use Class Derive

```csharp
public class TestCommand(PluginConfigData pluginConfig) : BasePlugin(pluginConfig)
{
    public override string Id => "Test";
    public override string Name => "Test Plugin";
    public override string[] TargetAdapter => ["OneBot11Adapter"];
    public override void Initialize()
    {
        RegisterCommand("test")
            .Description("指令帮助文档")
            .ShortDescription("帮助")
            .Usage("{0}test [xxx]")
            .Example("{0}test 114514")
            .Execute(async (ctx,source) => { })
            .Then(new VariableNode("var1", new IntegerArgument()));
    }
}
```

Use Source Generator

```csharp
[Plugin]
public partial class TestCommand
{
    public override string Id => "Test";
    public override string Name => "Test Plugin";
    public override string[] TargetAdapter => ["OneBot11Adapter"];
    public override void Initialize()
    {
        RegisterCommand("test")
            .Description("指令帮助文档")
            .ShortDescription("帮助")
            .Usage("{0}test [xxx]")
            .Example("{0}test 114514")
            .Execute(async (ctx,source) => { })
            .Then(new VariableNode("var1", new IntegerArgument()));
    }
}
```

- `Id` The plugin's unique identification
- `Name` The plugin's name
- `TargetAdapter` The adapters plugin will dock to

## Adapter Config

In `adapter.json`

```json5
{
    "Description": "Adapter for Homo",
    "EntryFile": "Adapter.Test",
    "GroupId" : [],
    "CommandPrefix" : "!"
    //other content
}
```

- `Description` The description of adapter
- `EntryFile` The binary assembly of adapter(Without Suffix)
- `GroupId` The groups adapter will be active in
- `CommandPrefix` The message with prefix will be processed
- You can write other content in this file. The custom content will be provided

## Adapter Develop

Adapter are developing with `UndefinedBot.Core` on Nuget or as git submodule.

Adapter's main class must not be abstract or static and

- Extend class `UndefinedBot.Core.Adapter.BaseAdapter` and
  override it's abstract fields and `HandleActionAsync` method and have a constructor with single param `AdapterDependencyCollection`.

- Mark class as `partial` and apply `PluginAttribute` to the class and do nothing on class declaration.Source Generator will implement the class.

Use `SubmitCommandEvent` to submit received command.

> Suggest to initialize adapter in `Initialize` method, rather than constructor.


Example:

Use Class Derive

```csharp
public class TestAdapter : BaseAdapter
{
    public override string Id => "Test";
    public override string Name => "Test Adapter";
    public override string Platform => "QQ";
    public override string Protocol => "OneBot11";
    public TestAdapter(AdapterConfigData adapterConfig) : base(adapterConfig)
}
```

Use Source Generator

```csharp
[Adapter]
public partial class TestAdapter : BaseAdapter
{
    public override string Id => "Test";
    public override string Name => "Test Adapter";
    public override string Platform => "QQ";
    public override string Protocol => "OneBot11";
    public TestAdapter(AdapterConfigData adapterConfig) : base(adapterConfig)
}
```

- `Id` The adapter's unique identification
- `Name` The adapter's name
- `Platform` The adapter' platform
- `Protocol` The adapter's protocol
