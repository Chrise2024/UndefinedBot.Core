# UndefinedBot.Core

UndefinedBot is a modularity QQ Bot based on .Net8.0 OneBot11.

In developing commands: [https://github.com/Chrise2024/UndefinedBot.Command.git](https://github.com/Chrise2024/UndefinedBot.Command.git)

## Plugins Loading

### Plugin

- Plugins are developing with `UndefinedBot.Core` on Nuget.

## What happened in loading plugins

- Main program start

- `Initializer.LoadPlugins()` is called

- `LoadPlugins()` will read `plugin.json` in plugin folder in `./Plugins` and get plugin's meta data

- load plugin assembly and create plugin instance accroding to plugin's meta data

- commands in plugin will be registered,creating command instance and generate command reference

- return plugin reference(including meta data and instance) to main program

- `Initializer.GetCommandReference()` is called

- `GetCommandReference()` will collect command reference and store them in program directory

- Start to Listen to http port

- Bot Launched

## Conbrtibuting

### Prerequisites

- .NET 8.0 or later

### Configuration

1. Clone the repository:
    ```sh
    git clone https://github.com/Chrise2024/UndefinedBot.Core.git
    ```
2. Navigate to the project directory:
    ```sh
    cd UndefinedBot.Core/UndefinedBot.Net
    ```
3. Restore the dependencies:
    ```sh
    dotnet restore
    ```

### Build&Run

1. Build the project:
    ```sh
    dotnet build
    ```
2. Run the project:
    ```sh
    dotnet run
    ```