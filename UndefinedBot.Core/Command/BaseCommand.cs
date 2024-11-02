using System;
using System.Reflection.Metadata;
using UndefinedBot.Core.Utils;

namespace UndefinedBot.Core.Command
{
    public interface IBaseCommand
    {
        public string CommandName { get; }
        public UndefinedAPI CommandApi { get; }
        public Task Handle(ArgSchematics args);
        public Task Execute(ArgSchematics args);
        public void Init();
    }
}
