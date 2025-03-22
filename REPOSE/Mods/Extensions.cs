using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Photon.Pun;
using REPOSE.Mods.Reflection;
using UnityEngine;

namespace REPOSE.Mods
{
    public static class Extensions
    {

        const BindingFlags ALL = (BindingFlags)(-1);

        /// <summary>
        /// Get all instances of MonoBehaviour type that have been recorded by Harmony.
        /// </summary>
        /// <typeparam name="T">The type to find</typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IReadOnlyList<T>? GetInstancesOf<T>(this Type type) where T : MonoBehaviour
        {
            if (!ModAggregator.typeInstances.TryGetValue(type, out List<MonoBehaviour> instances))
                return null; //return null if the type for some reason does not exist.

            return instances.OfType<T>().ToList(); //return all items recorded.
        }

        /// <summary>
        /// Helps in calling a PunRPC function. Refelectively finds a method of T type by name and calls it.
        /// </summary>
        /// <typeparam name="T">Instance type to call RPC on</typeparam>
        /// <param name="instance">Instance object</param>
        /// <param name="methodName">The name of the method that has a PunRPC attribute</param>
        /// <param name="target">The target Rpcs, default to All players.</param>
        /// <param name="photonView">The view to use. Leave default to automatically find one.</param>
        /// <param name="paramObjs">The parameter objects to pass to the method.</param>
        public static void RunRPC<T>(this T instance, string methodName, RpcTarget target = RpcTarget.All, PhotonView? photonView = default, params object[] paramObjs) where T : MonoBehaviour
        {
            if(typeof(T).GetMethod(methodName, ALL)?.GetCustomAttribute<PunRPC>() == null)
            {
                Logger.RepoDebugger.LogWarning($"Failed to get the appropriate method for running RPC: {methodName} on type {typeof(T).FullName}");
                return;
            }

            if(photonView == default)
            {
                PhotonView? reflectedPView = (PhotonView?)typeof(T).GetFields(ALL).FirstOrDefault(x => x.FieldType == typeof(PhotonView))?.GetValue(instance)
                ?? (PhotonView?)typeof(T).GetProperties(ALL).FirstOrDefault(p => p.PropertyType == typeof(PhotonView))?.GetValue(instance);

                if (reflectedPView == null)
                {
                    Logger.RepoDebugger.LogWarning($"Failed to get a photonView for type: {typeof(T).FullName}, please provide one.");
                    return;
                }

                photonView = reflectedPView;
            }

            photonView.RPC(methodName, target, paramObjs);
        }

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

        /// <summary>
        /// Gets a random item from a collection of items.
        /// </summary>
        /// <typeparam name="T">Items from a collection type</typeparam>
        /// <param name="items">All items of the type</param>
        /// <returns></returns>
        public static T GetRandom<T>(IEnumerable<T> items)
        {
            if (!items.Any())
                return default;
            else if (items.Count() == 1)
                return items.First();

            return items.ElementAt(UnityEngine.Random.Range(0, items.Count()));
        }
    }
}
