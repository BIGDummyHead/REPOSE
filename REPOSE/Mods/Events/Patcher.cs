using System;
using HarmonyLib;
using System.Reflection;

namespace REPOSE.Mods.Events
{
    public sealed class Patcher
    {
        const BindingFlags ALL = (BindingFlags)(-1);
        public Harmony HarmonyInstance { get; private set; }

        public Patcher(Harmony harmonyInstance)
        {
            HarmonyInstance = harmonyInstance;
        }

        /// <summary>
        /// Adds a patch to the following type's method (acquired via name), requires you to provide a pre/post fix method (delegate).
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="prefix"></param>
        /// <param name="postfix"></param>
        /// <exception cref="Exception"></exception>
        public void AddPatch(Type type, string methodName, Delegate? prefix = null, Delegate? postfix = null)
        {
            if (prefix == null && postfix == null)
                return; //nothing to do here, why waste resources.

            MethodInfo method = type.GetMethod(methodName, ALL) 
                ?? throw new Exception($"While patching type({type.FullName}), {methodName} returned null.");


            HarmonyMethod? prefixMethod = prefix == null ? null : new HarmonyMethod(prefix.Method ?? throw new Exception("The patching prefix was invalid"));
            HarmonyMethod? postfixMethod =  postfix == null ? null : new HarmonyMethod(postfix.Method ?? throw new Exception("The patching postfix was invalid"));
            HarmonyInstance.Patch(method, prefixMethod, postfixMethod);
        }
    }
}
