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

        /*
        
        using (FileStream fs = File.Open("C:\\Users\\shawn\\OneDrive\\Desktop\\testing.txt", FileMode.OpenOrCreate, FileAccess.Write))
		{
			using (StreamWriter writer = new StreamWriter(fs))
			{
				writer.WriteLine(Path.Combine(Directory.GetCurrentDirectory(), "REPOSE.dll"));
				writer.WriteLine("Loading assembly");
				Assembly assem = Assembly.LoadFrom(Path.Combine(Directory.GetCurrentDirectory(), "REPOSE.dll"));
				if (assem != null)
				{
					writer.WriteLine("Loaded assembly");
					writer.WriteLine("Loading type");
					Type t = assem.GetType("REPOSE.Mods.ModAggregator");
					writer.WriteLine("Loaded type: " + t.ToString());
					writer.WriteLine("Loading Method");
					MethodInfo mInfo = t.GetMethod("LoadAndStartMods");
					if (mInfo != null)
					{
						try
						{
							writer.WriteLine("Loaded Method");
							mInfo.Invoke(null, new object[0]);
							writer.WriteLine("Invoked method");
						}
						catch (Exception ex)
						{
							writer.WriteLine(ex);
						}
					}
				}
			}
		}

         */

        /// <summary>
        /// Default logger used.
        /// </summary>
//        private static ILog _defLogger = new ConsoleLogger();   //TODO: Replace this with something else.
        private static readonly ILog _defLogger = new ConsoleLogger();   //TODO: Replace this with something else.

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