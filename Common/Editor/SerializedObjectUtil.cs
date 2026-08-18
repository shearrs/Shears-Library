using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Shears.Editor
{
    public static class SerializedObjectUtil
    {
        public static T ReflectProperty<T>(this SerializedObject serializedObject, string name)
        {
            if (serializedObject == null || serializedObject.targetObject == null)
                return default;

            var type = serializedObject.targetObject.GetType();
            var propInfo = type.GetProperty(name);

            if (propInfo == null)
                return default;

            return (T)propInfo.GetValue(serializedObject.targetObject);
        }
    }
}
