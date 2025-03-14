namespace REPOSE.Mods
{
    /// <summary>
    /// Represents a Version for a mod, JSON serializable.
    /// </summary>
    public struct Version
    {
        /// <summary>
        /// 1.x.x.x
        /// </summary>
        public int major;

        /// <summary>
        /// x.1.x.x
        /// </summary>
        public int minor;

        /// <summary>
        /// x.x.1.x
        /// </summary>
        public int patch;

        /// <summary>
        /// x.x.x.1
        /// </summary>
        public int revision;

        /// <summary>
        /// Create a version with like: x.x.x.x
        /// </summary>
        /// <param name="major">1.x.x.x</param>
        /// <param name="minor">x.1.x.x</param>
        /// <param name="patch">x.x.1.x</param>
        /// <param name="revision">x.x.x.1</param>
        public Version(int major, int minor, int patch, int revision)
        {
            this.major = major;
            this.minor = minor;
            this.patch = patch;
            this.revision = revision;
        }

        public Version(string version)
        {
            string[] parts = version.Split('.');

            //Fixes the compiler error, whiny bitch
            major = 0;
            minor = 0;
            patch = 0;
            revision = 0;

            //make sure this is O(4)
            for (int i = 0; i < 4; i++)
            {
                //what is happening here?
                //1.1.2 -> is split into [1,1,2] (you notice how the 4th element is missing?
                //the 4th element is replaced with 0, since 4 goes past the parts length of 3
                string part = i >= parts.Length ? "0" : parts[i];

                if (!int.TryParse(part, out int ver))
                    ver = 0;

                switch (i)
                {
                    default:
                    case 0:
                        major = ver;
                        break;
                    case 1:
                        minor = ver;
                        break;
                    case 2:
                        patch = ver;
                        break;
                    case 3:
                        revision = ver;
                        break;
                }

            }
        }

        public override readonly string ToString()
        {
            return $"{major}.{minor}.{patch}.{revision}";
        }
    }
}
