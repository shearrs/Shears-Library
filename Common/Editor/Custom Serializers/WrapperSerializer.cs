using System.Collections.Generic;
using System.Reflection;
using Shears;
using Shears.Editor;
using Shears.Logging;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace KB.Editor
{
    public static class WrapperSerializer
    {
        private const string SCRIPT_FIELD_NAME = "m_Script";

        public static VisualElement Serialize(
            SerializedObject serializedObject,
            UnityEditor.Editor editor
        )
        {
            var root = new VisualElement() { name = $"{nameof(WrapperAttribute)} Editor" };

            InspectorElement.FillDefaultInspector(root, serializedObject, editor);

            var targetType = serializedObject.targetObject.GetType();
            var attribute = targetType.GetCustomAttribute<WrapperAttribute>();

            if (attribute == null)
                return root;

            var wrappedType = attribute.WrappedType;
            var insertIndex = attribute.SerializedFieldIndex;
            var fields = attribute.SerializedFields;

            var targetComponent = serializedObject.targetObject as Component;
            var wrappedComponent = targetComponent.GetComponent(wrappedType);

            if (wrappedComponent == null)
                return root;

            var wrappedSO = new SerializedObject(wrappedComponent);

            if (fields.Length == 0)
            {
                CollectionUtil.GetPooled(out List<string> iteratedFields);

                var iterator = wrappedSO.GetIterator();

                if (iterator.NextVisible(true))
                {
                    do
                    {
                        if (iterator.name != SCRIPT_FIELD_NAME)
                            iteratedFields.Add(iterator.name);
                    } while (iterator.NextVisible(false));
                }

                fields = iteratedFields.ToArray();

                CollectionUtil.ReleasePooled(iteratedFields);
            }

            int fieldOffset = 1; // Starts at 1 because of MonoBehaviour's m_Script field.

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var prop = wrappedSO.FindProperty(field);

                if (prop == null)
                {
                    SHLogger.LogWarning(
                        $"Could not find property '{field}' on type {wrappedType.Name}."
                    );
                    continue;
                }

                var propertyField = new PropertyField();
                propertyField.BindProperty(prop);

                VisualElement element = propertyField;

                if (i == 0 && !string.IsNullOrEmpty(attribute.Header))
                {
                    var container = new VisualElement();
                    var header = VisualElementEditorUtil.CreateHeader(attribute.Header);

                    container.AddAll(header, propertyField);

                    element = container;
                }

                int targetFieldIndex = insertIndex + fieldOffset;

                if (insertIndex == -1)
                    root.Add(element);
                else if (targetFieldIndex > root.childCount)
                {
                    SHLogger.Log(
                        $"{nameof(WrapperAttribute)} target field index {targetFieldIndex} is out of bounds, appending to end instead."
                    );
                    root.Add(element);
                }
                else
                {
                    root.Insert(targetFieldIndex, element);
                    fieldOffset++;
                }
            }

            return root;
        }

        public static void HideWrappedValue(Object target)
        {
            EditorApplication.delayCall += () => SetHideFlags(target);
        }

        public static void OnDestroy(Object target, Component wrappedValue)
        {
            if (target == null && wrappedValue != null)
            {
                var components = wrappedValue.GetComponents<Component>();
                var wrapperTypes = TypeCache.GetTypesWithAttribute<WrapperAttribute>();

                foreach (var component in components)
                {
                    var type = component.GetType();

                    if (wrapperTypes.Contains(type))
                    {
                        var attribute = type.GetCustomAttribute<WrapperAttribute>();

                        if (attribute.WrappedType == wrappedValue.GetType())
                            return;
                    }
                }

                if (Application.isPlaying)
                    Object.Destroy(wrappedValue);
                else
                    Undo.DestroyObjectImmediate(wrappedValue);
            }
        }

        private static void SetHideFlags(Object target)
        {
            if (target == null)
                return;

            var targetType = target.GetType();
            var attribute = targetType.GetCustomAttribute<WrapperAttribute>();

            if (attribute == null)
                return;

            var wrappedType = attribute.WrappedType;
            var targetComponent = target as Component;

            if (!targetComponent.TryGetComponent(wrappedType, out var wrappedComponent))
                return;

            var wrappedSO = new SerializedObject(wrappedComponent);
            var hideFlagsProp = wrappedSO.FindProperty("m_ObjectHideFlags");

            if (hideFlagsProp.intValue == (int)HideFlags.HideInInspector)
                return;

            hideFlagsProp.intValue = (int)HideFlags.HideInInspector;
            wrappedSO.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
