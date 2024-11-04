using System;

namespace UndefinedBot.Core.Utils
{
    public class Logger(string nameSpace)
    {
        private readonly string _nameSpace = nameSpace;

        private readonly ConsoleColor _defaultConsoleColor = Console.ForegroundColor;
        public void Error(string commandName,string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            PrintLine(commandName, message);
            Console.ForegroundColor = _defaultConsoleColor;
        }
        public void Warn(string commandName, string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            PrintLine(commandName, message);
            Console.ForegroundColor = _defaultConsoleColor;
        }
        public void Info(string commandName, string message)
        {
            Console.ForegroundColor = _defaultConsoleColor;
            PrintLine(commandName, message);
        }
        private void PrintLine(string commandName, string text)
        {
            string[] Lines = text.Split('\n',StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in Lines)
            {
                Console.WriteLine("[{0}][{1}][{2}] {3}", GetFormatTime(), _nameSpace, commandName,  line);
            }
        }
        private string GetFormatTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}