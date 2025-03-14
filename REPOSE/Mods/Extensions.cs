using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using REPOSE.Mods.Reflection;

namespace REPOSE.Mods
{
    public static class Extensions
    {
        public static string? GetSteamID(this PlayerAvatar player)
        {
            return player.GetField<string>("steamID");

        }

        public static PlayerVoiceChat GetVoiceChat(this PlayerAvatar player)
        {
            return player.GetField<PlayerVoiceChat>("voiceChat");
        }

        public static string? GetPlayerName(this PlayerAvatar player)
        {
            return player.GetField<string>("playerName");
        }

        public static bool IsDeveloperMode()
        {
            return SteamManager.instance.GetField<bool>("developerMode");
        }

        public static void ChangeDeveloperMode(bool state)
        {
            SteamManager.instance.SetField("developerMode", state);
        }
    }
}
