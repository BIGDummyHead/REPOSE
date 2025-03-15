using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using REPOSE.Logger;
using REPOSE.Mods.Events;
using UnityEngine.SceneManagement;

namespace REPOSE.Mods
{
    /// <summary>
    /// Mod aggregator, gathers things like Mod info and Mod dll and is able to load them. 
    /// </summary>
    public static class ModAggregator
    {
        const string HARMONY_ID = "com.REPOSE.Mods.dll";
        static readonly Harmony harmonyInstance = new Harmony(HARMONY_ID);

        const string MOD_JSON_IDENTIFIER = ".mod";

        /// <summary>
        /// The mods folder path Current Directory/Mods
        /// </summary>
        public static string ModsFolderPath => Path.Combine(Directory.GetCurrentDirectory(), "Mods");

        /// <summary>
        /// The directories of the Mods folder path
        /// </summary>
        public static string[] ModFolders => Directory.GetDirectories(ModsFolderPath);


        
        public static IReadOnlyList<Mod>? LoadedMods { get; private set; }
        private static int LoadAndStartMods()
        {
            if (!Harmony.HasAnyPatches(HARMONY_ID)) //initialize patching methods for harmony.
                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            LoadedMods ??= LoadMods();
            InitializeMods(); //we need to do this for main menu support

            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) =>
            {
                UninitializeMods();
                InitializeMods();
            };

            Debug.LogInfo("Successfully loaded mods!");
            Debug._defLogger.Dispose();

            //start a new console logger, allocates
            Debug._defLogger = new ConsoleLogger();

            Debug.LogInfo("Success Loaded Mods!\r\nThank you for downloading REPOSE. https://github.com/BIGDummyHead/REPOSE");

            return LoadedMods.Count;
        }

        // <summary>
        /// Initialize all loaded mods, is false if mods have not been loaded...
        /// </summary>
        /// <returns>True if LoadedMods was initialized. <see cref="LoadAndStartMods"/></returns>
        public static bool InitializeMods()
        {
            if (LoadedMods == null)
                return false;

            foreach (Mod mod in LoadedMods)
            {
                mod.Initialize();
            }

            return true;
        }

        /// <summary>
        /// Uninitialize all loaded mods, is false if mods have not been loaded...
        /// </summary>
        /// <returns>True if LoadedMods was initialized. <see cref="LoadAndStartMods"/></returns>
        public static bool UninitializeMods()
        {
            if (LoadedMods == null)
                return false;

            foreach (Mod mod in LoadedMods)
            {
                mod.UnInitialize();
            }

            return true;
        }

        /// <summary>
        /// This action cannot be undone, Aggregates all mods, loads their dll and info. DOES NOT INTIALIZE
        /// </summary>
        public static List<Mod> LoadMods()
        {
            List<Mod> mods = new List<Mod>();

            List<(string modInfoPath, Assembly modDLL)> aggreatedModItems = Aggregate();

            foreach (var modItem in aggreatedModItems)
            {
                if (string.IsNullOrEmpty(modItem.modInfoPath) || !File.Exists(modItem.modInfoPath) ||
                    modItem.modDLL == null)
                {
                    Debug.LogError($"There was an error reading a path to following mod: {modItem}");
                    continue;
                }
                string modInfoText = File.ReadAllText(modItem.modInfoPath);

                try
                {
                    Info desModInfo = JsonConvert.DeserializeObject<Info>(modInfoText);

                    //We want to use loadfrom, not load file.
                    //See here: https://stackoverflow.com/questions/1477843/difference-between-loadfile-and-loadfrom-with-net-assemblies
                    //Main reason for us is because if the mod dll relies on other DLLs, it does not import them.
                    //So dependencies for the mod get loaded.
                    //Assembly loadedModAssem = Assembly.LoadFrom(modItem.modDLLPath);

                    Type modType = modItem.modDLL.GetTypes().FirstOrDefault(type => type.IsSubclassOf(typeof(Mod)));

                    if (modType == null)
                    {
                        Debug.LogError($"The chosen assembly, did not contain a type with subclass of {nameof(Mod)}");
                        continue;
                    }

                    ConstructorInfo ctor = modType.GetConstructor(new Type[0]);

                    if (ctor == null)
                    {
                        Debug.LogError($"{modType.FullName} does not have an empty constructor. Please make an empty constructor.");
                        continue;
                    }

                    //finally a mod !yippee
                    Mod mod = (Mod)ctor.Invoke(new object[0]);
                    mod.info = desModInfo;
                    mods.Add(mod);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Could not load mod: '{modItem}'\r\n\tReason: {ex}");
                    continue;
                }
            }

            return mods;
        }

        /// <summary>
        /// Loads all valid directories (Mods) 
        /// </summary>
        /// <returns></returns>
        public static List<(string modPath, Assembly loadedDLL)> Aggregate()
        {
            List<(string modPath, Assembly loadedDLL)> aggregation = new List<(string, Assembly)>();

            foreach (string dir in ModFolders)
            {
                if (!Directory.Exists(dir))
                    continue; //skip over, since it does not exist for some reason now.

                string[] filePaths = Directory.GetFiles(dir);

                //Retrieve releavant mod files, .mod and .dlls
                string modInfoPath = filePaths.FirstOrDefault(x => Path.GetExtension(x).Equals(".mod", StringComparison.OrdinalIgnoreCase));
                IEnumerable<string> dllPaths = filePaths.Where(pth => Path.GetExtension(pth).Equals(".dll", StringComparison.OrdinalIgnoreCase));

                //Check for any errors inside of this folder. If so, skip over, we do not wanna ruin the rest of the mods.
                if (string.IsNullOrEmpty(modInfoPath))
                {
                    Debug.LogWarning($"Could not load directory '{dir}' because there was no *.mod file.");
                    continue;
                }
                else if (!dllPaths.Any())
                {
                    Debug.LogWarning($"Could not load directory '{dir}' because there was no *.dll file(s) to load.");
                    continue;
                }

                Assembly? modAssembly = null;
                foreach (string dllPath in dllPaths)
                {
                    if (!File.Exists(dllPath))
                        continue;

                    //get file data
                    //byte[] dllBytes = File.ReadAllBytes(dllPath);

                    //does not load the this into the currrent domain, meaning that we are just reading this file and not deps

                    //We want to use loadfrom, not load file.
                    //See here: https://stackoverflow.com/questions/1477843/difference-between-loadfile-and-loadfrom-with-net-assemblies
                    //Main reason for us is because if the mod dll relies on other DLLs, it does not import them.
                    //So dependencies for the mod get loaded.
                    Assembly currentAssembly = Assembly.LoadFrom(dllPath);

                    //Get any type matching
                    Type currentAssemblyMod = currentAssembly.GetTypes().FirstOrDefault(type => type.IsSubclassOf(typeof(Mod)));

                    if (currentAssemblyMod != null)
                    {
                        modAssembly = currentAssembly;
                        break;
                    }
                }

                if (modAssembly == null)
                {
                    Debug.LogWarning($"Could not load directory '{dir}' because there was no valid .dll types with a Mod inheritance.");
                    continue;
                }

                //We have thus confirmed that there is a "valid" .mod folder and a valid Assembly path
                //We are not sure if the Mod Info Path is valid, mainly because we did not read it, but we know it exist.
                aggregation.Add((modInfoPath, modAssembly));
            }

            //Return the list of successful items
            return aggregation;
        }


    }
}
