using System;
using System.IO;

namespace REPOSE.Logger
{


    public sealed class ConsoleLogger : ILog
    {
        public static string FormattedTime => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        public Stream WritingStream { get; set; }
        public Stream ReadingStream { get; set; }

        public ConsoleLogger()
        {
            WritingStream = Console.OpenStandardOutput();
            ReadingStream = Console.OpenStandardInput();
        }

        public void ChangeColor(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }

        public void Log(object message)
        {
            ChangeColor(ConsoleColor.White);
            System.Console.WriteLine(message);
        }

        public void LogWarning(object message)
        {
            ChangeColor(ConsoleColor.Yellow);
            System.Console.Write($"[WARNING - {FormattedTime}] ");
            ChangeColor(ConsoleColor.White);
            System.Console.WriteLine(message);
        }

        public void LogError(object message)
        {
            ChangeColor(ConsoleColor.Red);
            Console.Write($"[ERROR - {FormattedTime}] ");
            ChangeColor(ConsoleColor.White);
            System.Console.WriteLine(message);
        }

        public void LogInfo(object message)
        {
            ChangeColor(ConsoleColor.Blue);
            System.Console.Write($"[INFO - {FormattedTime}] ");
            ChangeColor(ConsoleColor.White);
            System.Console.WriteLine(message);
        }
    }

}
