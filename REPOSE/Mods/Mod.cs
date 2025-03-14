namespace REPOSE.Mods
{
    public abstract class Mod
    {
        public Mod() { }

        public Info info;

        
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

