using System.IO;
using Newtonsoft.Json;
using REPOSE.Logger;

namespace REPOSE.Mods
{
    /// <summary>
    /// Base class for creating a mod
    /// </summary>
    public abstract class Mod
    {
        public Mod() { }

        public Info? info;

        /// <summary>
        /// Converts the given file into a T type, JSON -> T
        /// </summary>
        /// <typeparam name="T">Conversion type</typeparam>
        /// <param name="path">Path of the file to read</param>
        /// <returns>A new Deserialized Object.</returns>
        public T LoadSettingsFile<T>(string path = "settings.json")
        {
            if(!File.Exists(path))
            {
                Debug.LogWarning($"Loading settings failed, '{path}' does not exist.");
                return default;
            }
            string text = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<T>(text);
        }
        
        /// <summary>
        /// Do everything you need to do like loading scripts.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Undo everything <see cref="Initialize"/> did.
        /// </summary>
        public abstract void UnInitialize();

        public override string ToString()
        {
            if (info == null)
                return "No mod information";

            return info.ToString();
        }
    }

}

