using System;
using System.IO;
using System.Runtime.InteropServices;

namespace REPOSE.Logger
{
    public sealed class ConsoleLogger : ILog
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        public static string FormattedTime => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        public Stream WritingStream { get; set; }
        public Stream ReadingStream { get; set; }

        public StreamWriter StandardOutput { get; set; }

        public ConsoleLogger()
        {
            AllocConsole();
            
            StandardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            WritingStream = StandardOutput.BaseStream;
            Console.SetOut(StandardOutput);
            Console.SetError(StandardOutput);
            ReadingStream = StandardOutput.BaseStream;

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

        public void Dispose()
        {
            FreeConsole();
        }
    }

}
