using System.IO;
using System;
using System.Reflection;

namespace REPOSE.Logger
{
    /// <summary>
    /// Debug class for logging and other useful information, to later be added.
    /// </summary>
    public static class Debug
    {
        /// <summary>
        /// Default logger used.
        /// </summary>
//        private static ILog _defLogger = new ConsoleLogger();   //TODO: Replace this with something else.
        internal static ILog _defLogger = new FileLogger(Path.Combine(Directory.GetCurrentDirectory(), "boot.log"));   //TODO: Replace this with something else.

        /// <summary>
        /// Uses the default logger to log a message.
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
            => _defLogger.Log(message);

        /// <summary>
        /// Uses the default logger to log a warning.
        /// </summary>
        /// <param name="message"></param>
        public static void LogWarning(string message)
            => _defLogger.LogWarning(message);

        /// <summary>
        /// Uses the default logger to log an error.
        /// </summary>
        /// <param name="message"></param>
        public static void LogError(string message)
            => _defLogger.LogError(message);

        /// <summary>
        /// Uses the default logger to log information.
        /// </summary>
        /// <param name="message"></param>
        public static void LogInfo(string message)
            => _defLogger.LogInfo(message);
    }
}