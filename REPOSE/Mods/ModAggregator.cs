using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using REPOSE.Logger;
using UnityEngine;
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

        private static bool modsLoaded = false;
        private static int LoadAndStartMods()
        {
            if (modsLoaded)
                return 0;

            modsLoaded = true;
            if (!Harmony.HasAnyPatches(HARMONY_ID))
            {
                TypeInstanceMonitor();
                //initialize patching methods for harmony.
                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }
            LoadedMods ??= LoadMods();
            
            RepoDebugger.LogInfo("Successfully loaded mods!");
            RepoDebugger._defLogger.Dispose();

            //start a new console logger, allocates
            RepoDebugger._defLogger = new ConsoleLogger();

            InitializeMods(true); //we need to do this for main menu support

            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) =>
            {
                UninitializeMods();
                InitializeMods();
            };

            

            RepoDebugger.LogInfo("Success Loaded Mods!\r\nThank you for downloading REPOSE. https://github.com/BIGDummyHead/REPOSE");

            return LoadedMods.Count;
        }

        internal static Dictionary<Type, List<MonoBehaviour>> typeInstances = new Dictionary<Type, List<MonoBehaviour>>();
        private static void TypeInstanceMonitor()
        {
            //choose some type to from the game's dll
            Assembly gameAssem = typeof(GameManager).Assembly;

            foreach (Type type in gameAssem.GetTypes()) //this may be pretty fuckin big
            {
                if (!type.IsSubclassOf(typeof(MonoBehaviour)))
                    continue; //ensure Mono

                BindingFlags allFlags = (BindingFlags)(-1);
                MethodInfo onAwake = type.GetMethod("Awake", allFlags);
                MethodInfo onDestroy = type.GetMethod("OnDestroy", allFlags);

                if (onAwake == null || onDestroy == null)
                    continue;

                MethodInfo preInfo = typeof(ModAggregator).GetMethod(nameof(HarmonyPrefix_TypeMonitor_Add), allFlags);
                MethodInfo postInfo = typeof(ModAggregator).GetMethod(nameof(HarmonyPrefix_TypeMonitor_Remove), allFlags);

                //patch the awake method!
                harmonyInstance.Patch(onAwake, new HarmonyMethod(preInfo));

                //patch the destroy method
                harmonyInstance.Patch(onDestroy, new HarmonyMethod(postInfo));
            }

        }

        private static void HarmonyPrefix_TypeMonitor_Add(MonoBehaviour __instance)
        {
            Type type = __instance.GetType();
            if (!typeInstances.ContainsKey(type)) //has the list already been added?
                typeInstances[type] = new List<MonoBehaviour>();

            typeInstances[type].Add(__instance);
        }

        private static void HarmonyPrefix_TypeMonitor_Remove(MonoBehaviour __instance)
        {
            Type type = __instance.GetType();

            if (typeInstances.ContainsKey(type))
                typeInstances[type].Remove(__instance);
        }

        // <summary>
        /// Initialize all loaded mods, is false if mods have not been loaded...
        /// </summary>
        public static bool InitializeMods(bool displayInfo = false)
        {
            if (LoadedMods == null)
                return false;

            foreach (Mod mod in LoadedMods)
            {
                if(displayInfo)
                    DisplayMod(mod);

                mod.Initialize();
            }

            return true;
        }

        private static void DisplayMod(Mod mod)
        {
            RepoDebugger.ChangeColor(ConsoleColor.Green);
            RepoDebugger.Log("== Mod Loaded ==");
            RepoDebugger.ChangeColor(ConsoleColor.White);
            RepoDebugger.Log(mod);
            RepoDebugger.ChangeColor(ConsoleColor.Green);
            RepoDebugger.Log("================");
        }

        /// <summary>
        /// Uninitialize all loaded mods, is false if mods have not been loaded...
        /// </summary>
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

            List<(Info modInfo, Assembly modDLL)> aggreatedModItems = Aggregate();

            foreach (var modItem in aggreatedModItems)
            {
                if (modItem.modDLL == null)
                {
                    RepoDebugger.LogError($"There was an error reading a path to following mod: {modItem}");
                    continue;
                }

                try
                {
                    Type modType = modItem.modDLL.GetTypes().FirstOrDefault(type => type.IsSubclassOf(typeof(Mod)));

                    if (modType == null)
                    {
                        RepoDebugger.LogError($"The chosen assembly, did not contain a type with subclass of {nameof(Mod)}");
                        continue;
                    }

                    ConstructorInfo ctor = modType.GetConstructor(new Type[0]);

                    if (ctor == null)
                    {
                        RepoDebugger.LogError($"{modType.FullName} does not have an empty constructor. Please make an empty constructor.");
                        continue;
                    }

                    //finally a mod !yippee
                    Mod mod = (Mod)ctor.Invoke(new object[0]);
                    mod.info = modItem.modInfo;
                    mods.Add(mod);
                }
                catch (Exception ex)
                {
                    RepoDebugger.LogError($"Could not load mod: '{modItem}'\r\n\tReason: {ex}");
                    continue;
                }
            }

            return mods;
        }

        private static Info? ReadModInfo(string path)
        {
            try
            {
                string text = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<Info>(text);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Loads all valid directories (Mods) 
        /// </summary>
        /// <returns></returns>
        public static List<(Info modInfo, Assembly loadedDLL)> Aggregate()
        {
            List<(Info modInfo, Assembly loadedDLL)> aggregation = new List<(Info, Assembly)>();

            foreach (string dir in ModFolders)
            {
                RepoDebugger.LogInfo($"Checking {dir} for mods");
                if (!Directory.Exists(dir))
                    continue; //skip over, since it does not exist for some reason now.

                string[] filePaths = Directory.GetFiles(dir);

                //Retrieve releavant mod files, .mod and .dlls
                string modInfoPath = filePaths.FirstOrDefault(x => Path.GetExtension(x).Equals(".mod", StringComparison.OrdinalIgnoreCase));

                Info? desModInfo = ReadModInfo(modInfoPath);

                if (desModInfo == null)
                {
                    RepoDebugger.LogError($"When trying to deserialize '{modInfoPath}' a problem ocurred");
                    continue;
                }

                IEnumerable<string> dllPaths = filePaths.Where(pth => Path.GetExtension(pth).Equals(".dll", StringComparison.OrdinalIgnoreCase));

                //Check for any errors inside of this folder. If so, skip over, we do not wanna ruin the rest of the mods.
                if (string.IsNullOrEmpty(modInfoPath))
                {
                    RepoDebugger.LogWarning($"Could not load directory '{dir}' because there was no *.mod file.");
                    continue;
                }
                else if (!dllPaths.Any() && string.IsNullOrEmpty(desModInfo.Value.DebugPath))
                {
                    RepoDebugger.LogWarning($"Could not load directory '{dir}' because there was no *.dll file(s) to load.");
                    continue;
                }

                


                Assembly? modAssembly = null;

                if (string.IsNullOrEmpty(desModInfo.Value.DebugPath) || !File.Exists(desModInfo.Value.DebugPath))
                {
                    RepoDebugger.LogInfo("Checking for dll files...");
                    foreach (string dllPath in dllPaths)
                    {
                        RepoDebugger.LogInfo($"{dllPath}");
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
                            RepoDebugger.LogInfo($"Chose assembly '{modAssembly.FullName}'");
                            break;
                        }
                    }
                }
                else
                {
                    modAssembly = Assembly.LoadFrom(desModInfo.Value.DebugPath);
                    RepoDebugger.LogInfo($"Chose debug assembly '{modAssembly.FullName}'");
                }


                if (modAssembly == null)
                {
                    RepoDebugger.LogWarning($"Could not load directory '{dir}' because there was no valid .dll types with a Mod inheritance.");
                    continue;
                }

                //We have thus confirmed that there is a "valid" .mod folder and a valid Assembly path
                //We are not sure if the Mod Info Path is valid, mainly because we did not read it, but we know it exist.
                aggregation.Add((desModInfo.Value, modAssembly));
            }

            //Return the list of successful items
            return aggregation;
        }


    }
}
