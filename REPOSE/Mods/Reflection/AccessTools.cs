using System;
using System.Reflection;

namespace REPOSE.Mods.Reflection
{
    public static class AccessTools
    {
        const BindingFlags ALL = (BindingFlags)(-1);

        /// <summary>
        /// Get a field's value, different from a property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T GetField<T>(this object obj, string fieldName)
        {
            if(obj == null) throw new ArgumentNullException(nameof(obj));

            Type type= obj.GetType();

            if (type == null)
                return default;

            return (T)type.GetField(fieldName, ALL).GetValue(obj);
        }

        /// <summary>
        /// Set a field's value, different from a property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void SetField<T>(this object obj, string fieldName, T value)
        {
            if(obj == null) throw new ArgumentNullException( nameof(obj));

            Type type = obj.GetType();

            if(type == null)
                return;

            FieldInfo field = type.GetField(fieldName);

            if (field == null)
                return;

            field.SetValue(obj, value);
        }

        /// <summary>
        /// Invoke a method and get the return type if not void.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="methodName"></param>
        /// <param name="paramObjs"></param>
        /// <returns></returns>
        public static object? InvokeMethod(this object obj, string methodName, params object[] paramObjs)
        {
            Type objType = obj.GetType();

            if (objType == null)
                return default;

            MethodInfo mInfo = objType.GetMethod(methodName, ALL);

            if (mInfo == null)
                return default;

            return mInfo.Invoke(obj, paramObjs);
        }

        /// <summary>
        /// Invoke a method and get the return type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="methodName"></param>
        /// <param name="paramObjs"></param>
        /// <returns></returns>
        public static T InvokeMethod<T>(this object obj, string methodName, params object[] paramObjs)
        {
            return (T)obj.InvokeMethod(methodName, paramObjs);
        }

        /// <summary>
        /// Set a property's value by name
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetProperty(this object obj, string propName, object value)
        {
            Type type = obj.GetType();

            if (type == null) return;

            PropertyInfo prop = type.GetProperty(propName, ALL);

            if(prop == null) return;    

            prop.SetValue(obj, value);
        }

        /// <summary>
        /// Get a property's value by name
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static object? GetProperty(this object obj, string propName)
        {
            Type type = obj.GetType();

            if (type == null) return null;

            PropertyInfo prop = type.GetProperty(propName, ALL);

            if (prop == null) return null;

            return prop.GetValue(obj);
        }

        /// <summary>
        /// Get a property's value by name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static T GetProperty<T>(this object obj, string propName)
        {
            return (T)GetProperty(obj, propName);
        }
    }
}
