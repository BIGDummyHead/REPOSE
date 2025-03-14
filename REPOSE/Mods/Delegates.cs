using System;
using System.Collections.Generic;
using System.Text;

namespace REPOSE.Mods
{
    /// <summary>
    /// Serves as a class to hold delegate members.
    /// </summary>
    public abstract class Delegates
    {
        public delegate bool OnChangeLevel(bool _completedLevel, bool _levelFailed, RunManager.ChangeLevelType _changeLevelType);
        public delegate bool OnPlayerDealth(PlayerAvatar player, int enemyIndex);
        public delegate bool OnPlayerRevive(PlayerAvatar player, bool byTruck);
        public delegate bool OnEnemySpawn(Enemy enemy);
        public delegate void OnEnemySpawnAfter(Enemy enemy, ref bool __result);
        public delegate bool OnTTSSpeakNow(TTSVoice __instance, ref string text, ref bool crouch);
    }
}
