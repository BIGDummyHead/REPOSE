using System;
using System.IO;

namespace REPOSE.Logger
{
    public sealed class FileLogger : ILog
    {
        public static string FormattedTime => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        public Stream WritingStream { get; set; }
        public Stream ReadingStream { get; set; }

        private readonly StreamWriter _writer;

        public FileLogger(string path)
        {
            WritingStream = ReadingStream = File.Create(path);
            _writer = new StreamWriter(WritingStream);
        }

        public void ChangeColor(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }

        public void Log(object message)
        {
            _writer.WriteLine(message);
        }

        public void LogWarning(object message)
        {
            _writer.WriteLine($"[WARNING - {FormattedTime}] {message}");
        }

        public void LogError(object message)
        {
            _writer.WriteLine($"[ERROR - {FormattedTime}] {message}");
        }

        public void LogInfo(object message)
        {
            _writer.WriteLine($"[INFO - {FormattedTime}] {message}");
        }

        public void Dispose()
        {
            _writer.Close();
            WritingStream.Close();
            ReadingStream.Close();
            _writer.Dispose();
        }
    }
}
