using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using System.Reflection;
using System.Linq.Expressions;

namespace REPOSE.Mods.Events
{
    public class Patcher
    {
        const BindingFlags ALL = (BindingFlags)(-1);
        public Harmony HarmonyInstance { get; private set; }

        public Patcher(Harmony harmonyInstance)
        {
            HarmonyInstance = harmonyInstance;
        }

        public void AddPatch(Type type, string methodName, Delegate prefix, Delegate postfix)
        {
            MethodInfo method = type.GetMethod(methodName, ALL) 
                ?? throw new Exception($"While patching type({type.FullName}), {methodName} returned null.");


            HarmonyMethod prefixMethod = new HarmonyMethod(prefix.Method ?? throw new Exception("The patching prefix was invalid"));
            HarmonyMethod postfixMethod = new HarmonyMethod(postfix.Method ?? throw new Exception("The patching postfix was invalid"));
            HarmonyInstance.Patch(method, prefixMethod, postfixMethod);
        }
    }
}
