using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Shears.Editor
{
    /// <summary>
    /// Extension methods for <see cref="SerializedProperty"/>."/>
    /// </summary>
    public static partial class SerializedPropertyUtil
    {
        // found from https://gist.github.com/monry/9de7009689cbc5050c652bcaaaa11daa
        /// <summary>
        /// Finds the parent property for the target <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="serializedProperty">The child to find a parent for.</param>
        /// <returns>The parent of the target <see cref="SerializedProperty"/>, or the default <see cref="SerializedProperty"/> value.</returns>
        public static SerializedProperty FindParentProperty(
            this SerializedProperty serializedProperty
        )
        {
            var propertyPaths = serializedProperty.propertyPath.Split('.');
            if (propertyPaths.Length <= 1)
            {
                return default;
            }

            var parentSerializedProperty = serializedProperty.serializedObject.FindProperty(
                propertyPaths.First()
            );
            for (var index = 1; index < propertyPaths.Length - 1; index++)
            {
                if (propertyPaths[index] == "Array")
                {
                    if (index + 1 == propertyPaths.Length - 1)
                    {
                        // reached the end
                        break;
                    }
                    if (
                        propertyPaths.Length > index + 1
                        && Regex.IsMatch(propertyPaths[index + 1], "^data\\[\\d+\\]$")
                    )
                    {
                        var match = Regex.Match(propertyPaths[index + 1], "^data\\[(\\d+)\\]$");
                        var arrayIndex = int.Parse(match.Groups[1].Value);
                        parentSerializedProperty = parentSerializedProperty.GetArrayElementAtIndex(
                            arrayIndex
                        );
                        index++;
                    }
                }
                else
                {
                    parentSerializedProperty = parentSerializedProperty.FindPropertyRelative(
                        propertyPaths[index]
                    );
                }
            }

            return parentSerializedProperty;
        }

        public static Type GetCollectionElementType(this SerializedProperty property)
        {
            if (property == null)
                return null;

            FieldInfo fieldInfo = GetFieldInfo(property);

            if (fieldInfo == null)
                return null;

            Type fieldType = fieldInfo.FieldType;

            if (fieldType.IsArray)
                return fieldType.GetElementType();

            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
                return fieldType.GetGenericArguments()[0];

            return null;
        }

        public static FieldInfo GetFieldInfo(this SerializedProperty property)
        {
            if (property == null)
                return null;

            Type currentType = property.serializedObject.targetObject.GetType();
            string[] pathSteps = property.propertyPath.Split('.');
            FieldInfo fieldInfo = null;

            for (int i = 0; i < pathSteps.Length; i++)
            {
                string step = pathSteps[i];

                if (
                    step == "Array"
                    && i + 1 < pathSteps.Length
                    && pathSteps[i + 1].StartsWith("data[")
                )
                {
                    if (currentType.IsArray)
                        currentType = currentType.GetElementType();
                    else if (
                        currentType.IsGenericType
                        && currentType.GetGenericTypeDefinition()
                            == typeof(System.Collections.Generic.List<>)
                    )
                        currentType = currentType.GetGenericArguments()[0];

                    i++;

                    continue;
                }

                fieldInfo = GetFieldIncludingBaseTypes(currentType, step);

                if (fieldInfo == null)
                    return null;

                currentType = fieldInfo.FieldType;
            }

            return fieldInfo;
        }

        private static FieldInfo GetFieldIncludingBaseTypes(Type type, string fieldName)
        {
            const BindingFlags flags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            while (type != null)
            {
                FieldInfo field = type.GetField(fieldName, flags);

                if (field != null)
                    return field;

                type = type.BaseType;
            }
            return null;
        }
    }
}
