using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using HarmonyLib;

namespace REPOSE.Mods.Assets
{
    public static class Loader
    {
        
        private static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private static readonly Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();
        private static readonly Dictionary<string, Texture2D> images = new Dictionary<string, Texture2D>();

       

        /// <summary>
        /// Register an Audio Clip for later
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileBaseDir">Base Directory</param>
        /// <param name="customName">A Custom Name To Be Accessed Under</param>
        public static void RegisterAudio(string fileName, string fileBaseDir, string? customName = null)
        {
            string audioPath = Path.Combine(fileBaseDir, fileName);

            customName ??= Path.GetFileNameWithoutExtension(audioPath);

            if (clips.ContainsKey(customName))
            {
                Debug.Log($"{customName} Has Already Been Registered!");
                return;
            }

            using (UnityWebRequest loaded = UnityWebRequestMultimedia.GetAudioClip(audioPath, GetAudioType(audioPath)))
            {
                loaded.SendWebRequest();

                while (!loaded.isDone)
                {

                }

                bool error = !string.IsNullOrEmpty(loaded.error);

                if (error)
                {
                    Debug.Log($"Failed To Load Your Audio! {customName}");
                    return;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(loaded);

                clips.Add(customName, clip);
            }
        }

        /// <summary>
        /// Get a Registered Audio Clip -> <see cref="RegisterAudio(string, string, string)"/>
        /// </summary>
        /// <param name="registeredName"></param>
        /// <returns></returns>
        public static AudioClip GetAudio(string registeredName)
        {
            if (clips.ContainsKey(registeredName))
            {
                return clips[registeredName];
            }

            Debug.Log($"Make Sure To Register {registeredName} Before Use");
            return default;
        }

        /// <summary>
        /// Get Audio From Your Registered Audio and Delegate an Action when it is loaded
        /// </summary>
        /// <param name="name"></param>
        /// <param name="onLoad"></param>
        public static void GetAudio(string name, Action<AudioClip> onLoad)
        {
            if (clips.ContainsKey(name))
            {
                onLoad.Invoke(GetAudio(name));
            }
            else
                Debug.Log("Register Audio First " + name);
        }

        /// <summary>
        /// Pass in a File path and this will determine an <see cref="AudioType"/>
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static AudioType GetAudioType(string filePath)
        {
            if (Path.HasExtension(filePath))
            {
                switch (Path.GetExtension(filePath))
                {
                    case ".wav":
                        return AudioType.WAV;
                    case ".mp3":
                        return AudioType.MPEG;
                    case ".mp2":
                        return AudioType.MPEG;
                    default:
                        break;
                }
            }
            return AudioType.UNKNOWN;
        }

        /// <summary>
        /// Register a Bundle 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileBaseDir">Base Directory</param>
        /// <param name="customName">A Custom Name To Accessed Under</param>
        public static void RegisterBundle(string fileName, string fileBaseDir, string? customName = null)
        {
            string full = Path.Combine(fileBaseDir, fileName);

            if (customName is null)
                customName = fileName;

            if (bundles.ContainsKey(customName))
            {
                Debug.Log("This Bundle Is Already Registered " + customName);
                return;
            }

            AssetBundle bundle = AssetBundle.LoadFromFileAsync(full).assetBundle;

            bundles.Add(customName, bundle);
        }

        /// <summary>
        /// Get The <see cref="RegisterBundle(string, string, string)"/> as an <seealso cref="AssetBundle"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static AssetBundle? GetBundle(string name)
        {
            if (!bundles.ContainsKey(name))
            {
                Debug.Log("Please Register " + name);
                return null;
            }

            return bundles[name];
        }

        /// <summary>
        /// Get a Bundle and pass in an action to be used On Load
        /// </summary>
        /// <param name="name"></param>
        /// <param name="onLoad"></param>
        public static void GetBundle(string name, Action<AssetBundle> onLoad)
        {
            if (bundles.ContainsKey(name))
                onLoad.Invoke(bundles[name]);
            else
                Debug.Log("Please Register " + name);
        }

        /// <summary>
        /// Get a Bundle
        /// </summary>
        /// <typeparam name="T">A <see cref="UnityEngine.Object"/></typeparam>
        /// <param name="name">The Name Of The Registered Bundle</param>
        /// <param name="instantiate">Instantiate The Bundle On Load - False Normally</param>
        /// <returns><typeparamref name="T"/></returns>
        public static T GetBundle<T>(string name, bool instantiate = false) where T : UnityEngine.Object
        {
            T? ran = null;

            GetBundle(name, delegate (AssetBundle bundle)
            {
                T obj = bundle.LoadAsset<T>(bundle.GetAllAssetNames()[0]);

                if (!instantiate)
                    ran = obj;
                else
                    ran = UnityEngine.Object.Instantiate(obj);
            });

            return ran;
        }

        /// <summary>
        /// Get A GameObject from a bundle - Instantiates
        /// </summary>
        /// <param name="name">The name of the bundle</param>
        /// <param name="instantiate">Instantiate - Normally True</param>
        /// <returns>A <see cref="UnityEngine.GameObject"/></returns>
        public static GameObject GetBundle(string name, bool instantiate = true) => GetBundle<GameObject>(name, instantiate);


        /// <summary>
        /// Register a Texture
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileBaseDir"></param>
        /// <param name="width">Width of the texture</param>
        /// <param name="height">Height of the texture</param>
        /// <param name="customName">Custom name to be accessed by</param>
        public static void RegisterTexture(string fileName, string fileBaseDir, int width = 500, int height = 500, string? customName = null)
        {
            string full = Path.Combine(fileBaseDir, fileName);

            if (customName is null)
                if (Path.HasExtension(full))
                    customName = Path.GetFileNameWithoutExtension(full);
                else
                    customName = fileName;

            if (images.ContainsKey(customName))
            {
                Debug.Log("This File Has Already Been Registered As An Image | " + customName);
                return;
            }

            Texture2D _text = new Texture2D(width, height, TextureFormat.RGBA32, false);

            if (_text.LoadImage(File.ReadAllBytes(full)))
            {
                images.Add(customName, _text);
                Debug.Log("Texture Registered " + customName);
            }
            else
                Debug.Log($"{customName} Could Not Be Registered");
        }

        /// <summary>
        /// Grab a Texture from your registered textures
        /// </summary>
        /// <param name="name"></param>
        /// <returns>an Image</returns>
        public static Texture2D? GetTexture(string name)
        {
            if (images.ContainsKey(name))
                return images[name];
            else
                Debug.Log("Please Register " + name);

            return null;
        }

    }
}
