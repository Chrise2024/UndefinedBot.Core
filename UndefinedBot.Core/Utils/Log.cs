using System;

namespace UndefinedBot.Core.Utils
{
    public class Logger(string nameSpace,string subType = "Common")
    {
        private readonly string _nameSpace = nameSpace;

        private readonly string _subType = subType;

        private readonly ConsoleColor _defaultConsoleColor = Console.ForegroundColor;
        public void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            PrintLine(message);
            Console.ForegroundColor = _defaultConsoleColor;
        }
        public void Warn(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            PrintLine(message);
            Console.ForegroundColor = _defaultConsoleColor;
        }
        public void Info(string message)
        {
            Console.ForegroundColor = _defaultConsoleColor;
            PrintLine(message);
        }
        private void PrintLine(string text)
        {
            string[] Lines = text.Split('\n',StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in Lines)
            {
                Console.WriteLine("[{0}][{1}][{2}] {3}", GetFormatTime(), _nameSpace, _subType, line);
            }
        }
        private string GetFormatTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}