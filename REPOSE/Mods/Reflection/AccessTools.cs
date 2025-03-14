using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace REPOSE.Mods.Reflection
{
    public static class AccessTools
    {
        const BindingFlags ALL = (BindingFlags)(-1);
        public static T GetField<T>(this object obj, string name)
        {
            if(obj == null) throw new ArgumentNullException(nameof(obj));

            Type type= obj.GetType();

            if (type == null)
                return default;

            return (T)type.GetField(name, ALL).GetValue(obj);
        }

        public static void SetField<T>(this object obj, string name, T value)
        {
            if(obj == null) throw new ArgumentNullException( nameof(obj));

            Type type = obj.GetType();

            if(type == null)
                return;

            FieldInfo field = type.GetField(name);

            if (field == null)
                return;

            field.SetValue(obj, value);
        }
    }
}
