using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Shears.Logging;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    public class InterfaceSerializer
    {
        private const string INTERFACE_ENTRIES_FIELD_NAME = "__interfaceEntries";

        /// <summary>
        /// Serialize the fields of the passed <see cref="SerializedObject"/>. Serializes fields normally, but also serializes interface fields.
        /// </summary>
        /// <param name="serializedObject">The target to serialize.</param>
        /// <returns>A <see cref="VisualElement"/> containing all of the serialized fields.</returns>
        public static VisualElement SerializeFields(SerializedObject serializedObject)
        {
            var root = new VisualElement() { name = $"{nameof(IInterfaceSerializable)} Editor" };
            var defaultFields = VisualElementEditorUtil.CreateDefaultFields(
                serializedObject,
                true,
                INTERFACE_ENTRIES_FIELD_NAME
            );

            var entryDictionaryProp = serializedObject.FindProperty(INTERFACE_ENTRIES_FIELD_NAME);
            var targetType = serializedObject.targetObject.GetType();

            SerializeFields(defaultFields, entryDictionaryProp, targetType);

            root.Add(defaultFields);

            return root;
        }

        /// <summary>
        /// Serialize the fields of the passed <see cref="SerializedProperty"/>. Serializes fields normally, but also serializes interface fields.
        /// </summary>
        /// <param name="property">The target to serialize.</param>
        /// <returns>A <see cref="VisualElement"/> containing all of the serialized fields.</returns>
        public static VisualElement SerializeFields(SerializedProperty property)
        {
            var root = new VisualElement() { name = $"{nameof(IInterfaceSerializable)} Editor" };

            if (
                property.propertyType == SerializedPropertyType.ManagedReference
                && property.managedReferenceValue == null
            )
                return root;

            var defaultFields = VisualElementEditorUtil.CreateDefaultFields(
                property,
                INTERFACE_ENTRIES_FIELD_NAME
            );

            var entryDictionaryProp = property.FindPropertyRelative(INTERFACE_ENTRIES_FIELD_NAME);
            var targetType = property.boxedValue.GetType();

            SerializeFields(defaultFields, entryDictionaryProp, targetType);

            root.Add(defaultFields);

            return root;
        }

        /// <summary>
        /// Serialize the entries in an <see cref="InterfaceDictionary"/> property and insert their fields into a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="fieldsContainer">The container for the fields.</param>
        /// <param name="entryDictionaryProp">The <see cref="SerializedProperty"/> for the <see cref="InterfaceDictionary"/>.</param>
        /// <param name="targetType">The target <see cref="Type"/> to serialize.</param>
        private static void SerializeFields(
            VisualElement fieldsContainer,
            SerializedProperty entryDictionaryProp,
            Type targetType
        )
        {
            var entriesProp = entryDictionaryProp.FindPropertyRelative("entries");

            CollectionUtil.GetPooled(out List<Type> types);
            CollectionUtil.GetPooled(out List<FieldInfo> serializedFields);
            CollectionUtil.GetPooled(out Dictionary<string, int> fieldIndex);

            var currentType = targetType;
            while (
                currentType != null
                && targetType != typeof(MonoBehaviour)
                && targetType != typeof(ScriptableObject)
            )
            {
                types.Add(currentType);
                currentType = currentType.BaseType;
            }

            types.Reverse();

            foreach (var type in types)
            {
                var fields = type.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );

                foreach (var field in fields)
                {
                    if (field.IsDefined(typeof(SerializeField), true))
                        serializedFields.Add(field);
                }
            }

            for (int i = 0; i < serializedFields.Count; i++)
            {
                var field = serializedFields[i];
                fieldIndex[field.Name] = i;
            }

            for (int i = 0; i < entriesProp.arraySize; i++)
            {
                var entryProp = entriesProp.GetArrayElementAtIndex(i);
                var keyProp = entryProp.FindPropertyRelative("key");
                var valueProp = entryProp.FindPropertyRelative("value");
                var valueField = new PropertyField(valueProp);
                valueField.BindProperty(valueProp);

                if (!fieldIndex.TryGetValue(keyProp.stringValue, out int index))
                {
                    SHLogger.LogError($"Failed to get index for field: {keyProp.stringValue}.");
                    continue;
                }

                index = Mathf.Min(index, fieldsContainer.childCount);

                VisualElement valueContainer = valueField;
                var field = serializedFields[index];
                var header = field.GetCustomAttribute<HeaderAttribute>();

                if (header != null)
                {
                    var container = new VisualElement() { name = "Interface Field Container" };
                    container.AddBaseFieldAlignClass();
                    var headerElement = VisualElementEditorUtil.CreateHeader(header.header);
                    headerElement.style.marginLeft = 3;

                    container.AddAll(headerElement, valueField);
                    valueContainer = container;
                }

                fieldsContainer.Insert(index, valueContainer);
            }

            CollectionUtil.ReleasePooled(types);
            CollectionUtil.ReleasePooled(serializedFields);
            CollectionUtil.ReleasePooled(fieldIndex);
        }
    }
}
