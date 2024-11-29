
namespace UndefinedBot.Core.Utils
{
    public class Logger(string nameSpace)
    {
        private readonly string _nameSpace = nameSpace;

        private readonly ConsoleColor _defaultConsoleColor = Console.ForegroundColor;
        /// <summary>
        /// Log errors on console
        /// </summary>
        /// <param name="message">message to log</param>
        public void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            PrintLine(message);
            Console.ForegroundColor = _defaultConsoleColor;
        }
        /// <summary>
        /// Log warns on console
        /// </summary>
        /// <param name="message">message to log</param>
        public void Warn(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            PrintLine(message);
            Console.ForegroundColor = _defaultConsoleColor;
        }
        /// <summary>
        /// Log infos on console
        /// </summary>
        /// <param name="message">message to log</param>
        public void Info(string message)
        {
            Console.ForegroundColor = _defaultConsoleColor;
            PrintLine(message);
        }
        private void PrintLine(string text)
        {
            string[] lines = text.Split('\n',StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                Console.WriteLine("[{0}][{1}] {2}", GetFormatTime(), _nameSpace, line);
            }
        }
        private string GetFormatTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
    public class CommandLogger(string namsSpace,string commandName)
    {
        private readonly ConsoleColor _defaultConsoleColor = Console.ForegroundColor;
        private readonly string _nameSpace = namsSpace;
        private readonly string _commandName = commandName;
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
            string[] lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                Console.WriteLine("[{0}][{1}][{2}] {3}", GetFormatTime(), _nameSpace, _commandName, line);
            }
        }
        private string GetFormatTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
