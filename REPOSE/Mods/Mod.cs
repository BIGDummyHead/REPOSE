using System.IO;
using Newtonsoft.Json;
using REPOSE.Logger;

namespace REPOSE.Mods
{
    public abstract class Mod
    {
        public Mod() { }

        public Info info;

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
    }

}

