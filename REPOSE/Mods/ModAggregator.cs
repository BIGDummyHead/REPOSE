using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Scripting.GarbageCollector;

namespace REPOSE.Mods
{
    /// <summary>
    /// Mod aggregator, gathers things like Mod info and Mod dll and is able to load them. 
    /// </summary>
    public static class ModAggregator
    {
        /// <summary>
        /// Controller for the console, allows you to show, hide, open, and free. Automatically set when mods are loaded.
        /// </summary>
        public static ConsoleController? ConsoleController {  get; private set; }

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

        public static IReadOnlyList<AggregatedMod>? LoadedMods => _loadedMods;



        private static List<AggregatedMod>? _loadedMods;

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

            _loadedMods ??= CreateModInstances();
            
            Debug.Log("Successfully loaded mods!");

            //Debug._defLogger.Dispose();

            ConsoleController = new ConsoleController();
            ConsoleController.ALlocate();
            //start a new console logger, allocates
            //Debug._defLogger = new ConsoleLogger();
            foreach (AggregatedMod mod in _loadedMods)
            {
                DisplayMod(mod.mod);
            }

            InitializeMods(); //we need to do this for main menu support

            /*SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) =>
            {
                UninitializeMods();
                InitializeMods();
            };*/

            Debug.Log("Success Loaded Mods!\r\nThank you for downloading REPOSE. https://github.com/BIGDummyHead/REPOSE");


            return LoadedMods.Count;
        }



        


        /// <summary>
        /// Initializes all mods that are in the LoadedMods
        /// </summary>
        /// <param name="displayInfo"></param>
        /// <returns></returns>
        public static bool InitializeMods()
        {
            if (LoadedMods == null)
                return false;

            foreach (AggregatedMod aggMod in LoadedMods)
            {
                aggMod.mod.Initialize();
            }

            return true;
        }

        /// <summary>
        /// Uninitialize all loaded mods, is false if mods have not been loaded...
        /// </summary>
        public static bool UninitializeMods()
        {
            if (LoadedMods == null)
                return false;

            foreach (AggregatedMod aggMod in LoadedMods)
            {
                aggMod.mod.UnInitialize();
            }

            return true;
        }

        /// <summary>
        /// If mods are loaded then calls the the Uninitialize and the Initialize methods to restart the mods.
        /// </summary>
        public static void ReloadMods()
        {
            if(!modsLoaded) return;

            UninitializeMods();
            InitializeMods();
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

        

       
        public static List<AggregatedMod> CreateModInstances()
        {
            List<AggregatedMod> mods = new List<AggregatedMod>();
            foreach (string modFolder in ModFolders)
            {
                AggregatedMod aggMod = new AggregatedMod();

                string[] files = Directory.GetFiles(modFolder);

                string modInfoFile = files.FirstOrDefault(f => Path.GetExtension(f).Equals(MOD_JSON_IDENTIFIER));

                if(string.IsNullOrEmpty(modInfoFile))
                {
                    Debug.LogError($"{modFolder} does not a {MOD_JSON_IDENTIFIER} file.");
                    continue;
                }

                Info? posInfo = ReadModInfo(modInfoFile);
                if (posInfo == null)
                {
                    Debug.LogError($"{modFolder} had an invalid Mod info file.");
                    continue;
                }
                aggMod.info = posInfo.Value;

                if (aggMod.info.IsDebug) //simply load the debug path file...
                {
                    aggMod.dllPath = aggMod.info.DebugPath;
                    aggMod.assembly = Assembly.LoadFrom(aggMod.dllPath);
                    aggMod.loadedAssemblies = new List<Assembly>(new Assembly[1]{ aggMod.assembly });

                    if(!CreateModInstance(aggMod.assembly, aggMod))
                    {
                        Debug.LogWarning($"{aggMod.info.DebugPath} was not a valid mod dll.");
                        continue;
                    }
                    else
                    {
                        Debug.Log($"Loaded mod from debug path, {aggMod.mod}");
                    }
                }
                else //is not a debug path 
                {
                    IEnumerable<string> dllFiles = files.Where(f => Path.GetExtension(f).Equals(".dll"));
                    if (!dllFiles.Any())
                    {
                        Debug.LogError($"{modFolder} does not have any .dll files.");
                        continue;
                    }

                    aggMod.loadedAssemblies = new List<Assembly>();
                    Assembly? modAssembly = null;
                    foreach (string assemFile in dllFiles)
                    {
                        Assembly current = Assembly.LoadFrom(assemFile);

                        if (modAssembly == null && CreateModInstance(current, aggMod))
                        {
                            modAssembly = current;
                        }

                        aggMod.loadedAssemblies.Add(current);
                    }
                }

                Debug.Log($"Loaded mod: {aggMod.mod}");
                mods.Add(aggMod);
                DisplayMod(aggMod.mod);
            }

            return mods;
        }

        private static bool CreateModInstance(Assembly current, AggregatedMod aggMod)
        {
            Type? modType = current.GetTypes().FirstOrDefault(type =>
            type.IsSubclassOf(typeof(Mod))
            && type.GetConstructor(new Type[0]) != null);

            if (modType == null)
                return false;
            //modAssembly = current;
            aggMod.dllPath = current.CodeBase;
            aggMod.mod = (Mod)Activator.CreateInstance(modType);
            aggMod.mod.assembly = current;
            aggMod.mod.info = aggMod.info;


            return true;
        }

        private static void DisplayMod(Mod mod)
        {
            Debug.Log("== Mod Loaded ==");
            Debug.Log(mod);
            Debug.Log("================");
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



        public class AggregatedMod
        {
            public Mod mod;
            public Info info;
            public Assembly assembly;
            public string dllPath;
            public List<Assembly> loadedAssemblies;
            //other info that is neccessary 
        }
    }
}
