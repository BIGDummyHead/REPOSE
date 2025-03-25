using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace REPOSE.Mods
{
    /// <summary>
    /// Console allocation class, deals with helping control the console window.
    /// </summary>
    public sealed class ConsoleController : IDisposable
    {
        public bool ConsoleOpen { get; private set; }

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();


        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();


        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr win, int i);


        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();


        const int SHOW_CONSOLE = 5;
        const int HIDE_CONSOLE = 0;



        /// <summary>
        /// Allocate and open the console.
        /// </summary>
        /// <returns>True if allocation was successful</returns>
        public bool ALlocate()
        {
            if (ConsoleOpen)
            {
                Console.Write("Console is already open");
                return false;
            }
            else if (!AllocConsole())
                return false;

            StreamWriter streamWriter = new StreamWriter(Console.OpenStandardOutput());
            streamWriter.AutoFlush = true;
            Console.SetError(streamWriter);
            Console.SetOut(streamWriter);

            Application.logMessageReceived += LogMessage;
            ConsoleOpen = true;
            return true;
        }

        /// <summary>
        /// Frees the console
        /// </summary>
        /// <returns>True if console is open and console was successfully freed</returns>
        public bool Free()
        {
            if (!ConsoleOpen) return false;

            ConsoleOpen = !FreeConsole();
            return ConsoleOpen;
        }

        /// <summary>
        /// Show the console
        /// </summary>
        /// <returns>True if the console is open and ShowWindow is successfully called</returns>
        public bool Show()
        {
            if (!ConsoleOpen)
                return false;

            IntPtr handle = GetConsoleWindow();
            return ShowWindow(handle, SHOW_CONSOLE);
        }

        /// <summary>
        /// Hide the console, different from freeing
        /// </summary>
        /// <returns>True if the console is open and ShowWindow is successful</returns>
        public bool Hide()
        {
            if (!ConsoleOpen)
                return false;

            IntPtr h = GetConsoleWindow();
            return ShowWindow(h, HIDE_CONSOLE);
        }

        /// <summary>
        /// Used for unity logging purposes
        /// </summary>
        /// <param name="log"></param>
        /// <param name="trace"></param>
        /// <param name="type"></param>
        private static void LogMessage(string log, string trace, LogType type)
        {
            if (string.IsNullOrEmpty(log))
                return;

            if (string.IsNullOrEmpty(trace))
                trace = "Unknown Trace";

            ConsoleColor prevColor = Console.ForegroundColor;
            ConsoleColor prevBGClr = Console.BackgroundColor;
            Console.ForegroundColor = GetConsoleColor(type);
            Console.BackgroundColor = ConsoleColor.Black;

            switch (type)
            {
                case LogType.Error:
                    Console.Write($"ERROR [{trace}]: ");
                    break;
                case LogType.Assert:
                    Console.Write("ASSERTION: ");
                    break;
                case LogType.Warning:
                    Console.Write("WARNING: ");
                    break;
                case LogType.Exception:
                    Console.Write($"EXCEPTION [{trace}]: ");
                    break;
            }
            Console.BackgroundColor = prevBGClr;
            Console.ForegroundColor = prevColor;


            Console.WriteLine(log + "\r\n");
        }

        private static ConsoleColor GetConsoleColor(LogType type)
            => type switch
            {
                LogType.Error => ConsoleColor.Red,
                LogType.Assert => ConsoleColor.DarkYellow,
                LogType.Warning => ConsoleColor.Yellow,
                LogType.Exception => ConsoleColor.DarkRed,
                _ => ConsoleColor.White,
            };

        /// <summary>
        /// If the console has been opened, Free.
        /// </summary>
        public void Dispose()
        {
            Free();
        }
    }
}
