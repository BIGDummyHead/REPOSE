using System;
using System.IO;

namespace REPOSE.Logger
{
    /// <summary>
    /// Interface for logging.
    /// </summary>
    public interface ILog : IDisposable
    {

        /// <summary>
        /// Stream to write to.
        /// </summary>
        public Stream WritingStream { get; set; }

        /// <summary>
        /// Stream to read from, if any.
        /// </summary>
        public Stream ReadingStream { get; set; }

        /// <summary>
        /// Changes the color of the console, if possible.
        /// </summary>
        /// <param name="color">Color to use.</param>

        void ChangeColor(ConsoleColor color);

        /// <summary>
        ///  Logs a message. Without any additional information.
        /// </summary>
        /// <param name="message">Message to display.</param>
        void Log(object message);

        /// <summary>
        /// Logs a message as a warning.
        /// </summary>
        /// <param name="message">The message to display.</param>
        void LogWarning(object message);

        /// <summary>
        /// Logs a message as an error.
        /// </summary>
        /// <param name="message">The message to display.</param>
        void LogError(object message);

        /// <summary>
        /// Logs a message as information.
        /// </summary>
        /// <param name="message">The message to display.</param>
        void LogInfo(object message);

    }
}