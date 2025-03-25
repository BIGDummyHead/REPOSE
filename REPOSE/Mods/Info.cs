using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace REPOSE.Mods
{
    [Serializable]
    public struct Info
    {
        /// <summary>
        /// Name of your mod, loaded from a JSON file
        /// </summary>
        [JsonProperty]
        public string Name { get; set; }

        /// <summary>
        /// Name of the creator of the mod.
        /// </summary>
        [JsonProperty]
        public string Author { get; set; }

        /// <summary>
        /// Description of your mod, loaded from a JSON file.
        /// </summary>
        [JsonProperty]
        public string Description { get; set; }

        /// <summary>
        /// Version of your mod, loaded from your mod
        /// </summary>
        [JsonIgnore]
        public Version Version => 
                 _convVersion ??= new Version(_version);

        [JsonProperty("Version")]
        private string _version;

        [JsonIgnore]
        private Version? _convVersion;

        [JsonProperty("debug_path")]
        public string DebugPath { get; set; }

        public readonly bool IsDebug => !string.IsNullOrEmpty(DebugPath);

        public override string ToString()
        {
            string top = $"{Name} created by {Author}\r\n{Description}\r\nVersion: {Version}";

            if (!string.IsNullOrEmpty(DebugPath))
                return top + $"\r\nDebug at: {DebugPath}";

            return top;
        }
    }
}
