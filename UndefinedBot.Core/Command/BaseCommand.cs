using System;
using System.Reflection.Metadata;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Command
{
    [Obsolete("Decperated")]
    public interface IBaseCommand
    {
        public string PluginName { get; }
        public string CommandName { get; }
        public List<string> CommandNameAlias { get; }
        public UndefinedAPI CommandApi { get; }
        public Task Handle(ArgSchematics args);
        public Task Execute(ArgSchematics args);
        public void Init();
    }
}
