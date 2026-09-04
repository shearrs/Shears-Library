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
        public static VisualElement SerializeFields(SerializedObject serializedObject)
        {
            var root = new VisualElement() { name = $"{nameof(IInterfaceSerializable)} Editor" };
            var defaultFields = VisualElementEditorUtil.CreateDefaultFields(
                serializedObject,
                true,
                "__interfaceEntries"
            );

            var entryDictionaryProp = serializedObject.FindProperty("__interfaceEntries");
            var entriesProp = entryDictionaryProp.FindPropertyRelative("entries");

            var targetType = serializedObject.targetObject.GetType();
            var serializedFields = targetType
                .GetFields(
                    BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic
                        | BindingFlags.FlattenHierarchy
                )
                .Where(field => field.IsDefined(typeof(SerializeField), true))
                .ToArray();

            CollectionUtil.GetPooled(out Dictionary<string, int> fieldIndex);

            for (int i = 0; i < serializedFields.Length; i++)
            {
                var field = serializedFields[i];
                fieldIndex[field.Name] = i + 1;
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

                index = Mathf.Min(index, defaultFields.childCount);

                defaultFields.Insert(index, valueField);
            }

            CollectionUtil.ReleasePooled(fieldIndex);

            root.Add(defaultFields);

            return root;
        }

        public static VisualElement SerializeFields(SerializedProperty property)
        {
            var root = new VisualElement() { name = $"{nameof(IInterfaceSerializable)} Editor" };
            var defaultFields = VisualElementEditorUtil.CreateDefaultFields(
                property,
                "__interfaceEntries"
            );

            var entryDictionaryProp = property.FindPropertyRelative("__interfaceEntries");
            var entriesProp = entryDictionaryProp.FindPropertyRelative("entries");

            for (int i = 0; i < entriesProp.arraySize; i++)
            {
                var entryProp = entriesProp.GetArrayElementAtIndex(i);
                var valueProp = entryProp.FindPropertyRelative("value");
                var valueField = new PropertyField(valueProp);
                valueField.BindProperty(valueProp);

                defaultFields.Add(valueField);
            }

            root.Add(defaultFields);

            return root;
        }
    }
}
