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

        /// <summary>
        /// Find a C# property <see cref="SerializedProperty"/>. Uses the name to search for the generated backing field.
        /// </summary>
        /// <param name="serializedObject">The <see cref="SerializedObject"/> to search through.</param>
        /// <param name="propertyPath">The path of the <see cref="SerializedProperty"/>.</param>
        /// <returns>The found <see cref="SerializedProperty"/></returns>
        public static SerializedProperty FindAutoProperty(
            this SerializedObject serializedObject,
            string propertyPath
        )
        {
            return serializedObject.FindProperty($"<{propertyPath}>k__BackingField");
        }
    }
}
