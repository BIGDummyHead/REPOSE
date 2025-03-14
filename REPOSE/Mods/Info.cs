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
    }
}
