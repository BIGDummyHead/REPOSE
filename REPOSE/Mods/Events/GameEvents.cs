using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
namespace REPOSE.Mods.Events
{

    [HarmonyPatch]
    public static class GameEvents
    {
        public static event Delegates.OnChangeLevel? BeforeLevelChanged;
        public static event Delegates.OnChangeLevel? AfterLevelChanged;

        public static event Delegates.OnPlayerDealth? BeforePlayerDeath;
        public static event Delegates.OnPlayerDealth? AfterPlayerDeath; 

        public static event Delegates.OnPlayerRevive? BeforePlayerRevive;
        public static event Delegates.OnPlayerRevive? AfterPlayerRevive;

        public static event Delegates.OnEnemySpawn? BeforeEnemySpawn;
        public static event Delegates.OnEnemySpawnAfter? AfterEnemySpawn;


        

        [HarmonyPatch(typeof(RunManager), nameof(RunManager.ChangeLevel))]
        [HarmonyPrefix]
        private static bool Prefix_ChangeLevel(bool _completedLevel, bool _levelFailed, RunManager.ChangeLevelType _changeLevelType)
        {
            return BeforeLevelChanged == null || BeforeLevelChanged.Invoke(_completedLevel, _levelFailed, _changeLevelType);
        }

        [HarmonyPatch(typeof(RunManager), nameof(RunManager.ChangeLevel))]
        [HarmonyPostfix]
        private static void Postfix_ChangeLevel(bool _completedLevel, bool _levelFailed, RunManager.ChangeLevelType _changeLevelType)
        {
            AfterLevelChanged?.Invoke(_completedLevel, _levelFailed, _changeLevelType);
        }

        [HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.PlayerDeath))]
        [HarmonyPrefix]
        private static bool Prefix_PlayerDied(PlayerAvatar __instance, int enemyIndex)
        {
            return BeforePlayerDeath == null || BeforePlayerDeath.Invoke(__instance, enemyIndex);
        }

        [HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.PlayerDeath))]
        [HarmonyPostfix]
        private static void Postfix_PlayerDied(PlayerAvatar __instance, int enemyIndex)
        {
            AfterPlayerDeath?.Invoke(__instance, enemyIndex);
        }

        [HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.Revive))]
        [HarmonyPrefix]
        private static bool Prefix_PlayerRevive(PlayerAvatar __instance, bool _revivedByTruck)
        {
            return BeforePlayerRevive == null || BeforePlayerRevive.Invoke(__instance, _revivedByTruck);
        }

        [HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.Revive))]
        [HarmonyPostfix]
        private static void Postfix_PlayerRevive(PlayerAvatar __instance, bool _revivedByTruck)
        {
           AfterPlayerRevive?.Invoke(__instance, _revivedByTruck);
        }

        [HarmonyPatch(typeof(SemiFunc), nameof(SemiFunc.EnemySpawn))]
        [HarmonyPrefix]
        private static bool Prefix_EnemySpawn(Enemy enemy)
        {
            return BeforeEnemySpawn == null || BeforeEnemySpawn.Invoke(enemy);
        }

        [HarmonyPatch(typeof(SemiFunc), nameof(SemiFunc.EnemySpawn))]
        [HarmonyPostfix]
        private static void Postfix_EnemySpawn(Enemy enemy, ref bool __result)
        {
            AfterEnemySpawn?.Invoke(enemy, ref __result);
        }

        public static event Delegates.OnTTSSpeakNow? BeforeTTSSpeakNow;
        public static event Delegates.OnTTSSpeakNow? AfterTTSSpeakNow;

        [HarmonyPatch(typeof(TTSVoice), nameof(TTSVoice.TTSSpeakNow))]
        [HarmonyPrefix]
        private static bool Prefix_TTSSpeakNow(TTSVoice __instance, ref string text, ref bool crouch)
        {
            return BeforeTTSSpeakNow == null || BeforeTTSSpeakNow.Invoke(__instance, ref text, ref crouch);
        }

        [HarmonyPatch(typeof(TTSVoice), nameof(TTSVoice.TTSSpeakNow))]
        [HarmonyPostfix]
        private static void Post_TTSSpeakNow(TTSVoice __instance, ref string text, ref bool crouch)
        {
            AfterTTSSpeakNow?.Invoke(__instance, ref text, ref crouch);
        }

        public static event Delegates.OnPlayerSpawn? BeforePlayerSpawn;
        public static event Delegates.OnPlayerSpawn? AfterPlayerSpawn;

        const string PLAYER_SPAWN_METHOD_NAME = "Awake";

        [HarmonyPatch(typeof(PlayerAvatar), PLAYER_SPAWN_METHOD_NAME)]
        [HarmonyPrefix]
        private static bool Prefix_PlayerSpawn(PlayerAvatar __instance)
        {
            return BeforePlayerSpawn == null || BeforePlayerSpawn.Invoke(__instance);
        }

        [HarmonyPatch(typeof(PlayerAvatar), PLAYER_SPAWN_METHOD_NAME)]
        [HarmonyPostfix]
        private static void Postfix_PlayerSpawn(PlayerAvatar __instance)
        {
            AfterPlayerSpawn?.Invoke(__instance);
        }

    }
}
