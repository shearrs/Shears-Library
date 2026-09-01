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
                return default;

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

        /// <summary>
        /// Get the generic arguments from a generic (as in &lt;T&gt;) <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="property">The property to read.</param>
        /// <returns>The collection of generic arguments.</returns>
        public static Type[] GetGenericArguments(this SerializedProperty property)
        {
            var fieldInfo = property.GetFieldInfo();

            return fieldInfo.FieldType.GetGenericArguments();
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

        /// <summary>
        /// Get the reflected <see cref="FieldInfo"/> of a <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="property">The property to read.</param>
        /// <returns>The property's <see cref="FieldInfo"/>.</returns>
        public static FieldInfo GetFieldInfo(this SerializedProperty property)
        {
            if (property == null)
                return null;

            var targetType = property.serializedObject.targetObject.GetType();
            string[] pathSteps = property.propertyPath.Split('.');
            var currentProperty = property.serializedObject.FindProperty(pathSteps[0]);
            FieldInfo fieldInfo = null;

            for (int i = 0; i < pathSteps.Length; i++)
            {
                string step = pathSteps[i];

                if (step == "Array")
                {
                    i++;

                    if (fieldInfo != null && fieldInfo.FieldType.IsGenericType)
                        targetType = fieldInfo.FieldType.GetGenericArguments()[0];
                    else if (fieldInfo != null && fieldInfo.FieldType.IsArray)
                        targetType = fieldInfo.FieldType.GetElementType();

                    if (currentProperty.arraySize == 0)
                        currentProperty = null;
                    else
                    {
                        var indexStep = pathSteps[i];
                        int index = int.Parse(indexStep.Substring(indexStep.IndexOf('[') + 1, 1));

                        currentProperty = currentProperty.GetArrayElementAtIndex(index);
                    }

                    continue;
                }

                if (i > 0 && currentProperty != null)
                    currentProperty = currentProperty.FindPropertyRelative(step);

                if (
                    currentProperty != null
                    && currentProperty.propertyType == SerializedPropertyType.ManagedReference
                )
                {
                    if (currentProperty.managedReferenceValue == null)
                        return null;
                    else
                        targetType = currentProperty.managedReferenceValue.GetType();
                }
                else
                {
                    fieldInfo = GetFieldIncludingBaseTypes(targetType, step);

                    if (fieldInfo == null)
                        return null;

                    targetType = fieldInfo.FieldType;
                }
            }

            return fieldInfo;
        }

        /// <summary>
        /// Get the reflected <see cref="FieldInfo"/> of a field on a given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The type to search for the field.</param>
        /// <param name="fieldName">The name of the field to search for.</param>
        /// <returns>The <see cref="FieldInfo"/> of the passed field.</returns>
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

        /// <summary>
        /// Get a custom attribute type from a <see cref="SerializedProperty"/>.
        /// </summary>
        /// <typeparam name="T">The type of attribute to get.</typeparam>
        /// <param name="property">The property to search.</param>
        /// <returns>The custom attribute if it exists.</returns>
        public static T GetAttribute<T>(this SerializedProperty property)
            where T : Attribute
        {
            var fieldInfo = property.GetFieldInfo();

            return fieldInfo?.GetCustomAttribute<T>(true);
        }

        public static T ReflectProperty<T>(this SerializedProperty serializedProperty, string name)
        {
            if (serializedProperty == null || serializedProperty.boxedValue == null)
                return default;

            var type = serializedProperty.boxedValue.GetType();
            var propInfo = type.GetProperty(
                name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy
            );

            if (propInfo == null)
                return default;

            return (T)propInfo.GetValue(serializedProperty.boxedValue);
        }
    }
}
