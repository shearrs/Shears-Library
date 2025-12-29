using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomEditor(typeof(ManagedWrapper), true)]
    public class ManagedWrapperEditor : UnityEditor.Editor
    {
        protected Component wrappedValue;

        protected virtual void OnEnable()
        {
            if (wrappedValue != null)
                return;

            var wrapper = target as ManagedWrapper;

            if (wrapper == null)
                return;

            wrappedValue = wrapper.WrappedValue;

            EditorApplication.delayCall += SetHideFlags;
        }

        private void OnDisable()
        {
            EditorApplication.delayCall -= SetHideFlags;
        }

        private void SetHideFlags()
        {
            var wrappedValueSO = new SerializedObject(wrappedValue);
            wrappedValueSO.FindProperty("m_ObjectHideFlags").intValue = (int)HideFlags.HideInInspector;
            wrappedValueSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(wrappedValueSO.targetObject);
        }

        protected virtual void OnDestroy()
        {
            if (target == null && wrappedValue != null)
            {
                var wrappers = wrappedValue.GetComponents<ManagedWrapper>();

                foreach (var wrapper in wrappers)
                {
                    if (wrapper.WrappedValue == wrappedValue)
                        return;
                }

                if (Application.isPlaying)
                    Destroy(wrappedValue);
                else
                    DestroyImmediate(wrappedValue);
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var wrappedValueSO = new SerializedObject(wrappedValue);
            var wrapperType = target.GetType();

            var defaultFields = VisualElementUtil.CreateDefaultFields(serializedObject);
            var wrappedFields = VisualElementUtil.CreateDefaultFields(wrappedValueSO);

            if (TryGetAttribute(wrapperType, out var attribute))
            {
                if (attribute.ShowAllFields)
                {
                    RemoveChildWithName("m_Script", wrappedFields);

                    root.AddAll(defaultFields, wrappedFields);
                }
                else if (attribute.DisplayFields != null)
                {
                    var displayFieldsContainer = new VisualElement();
                    RemoveChildWithName("m_Script", wrappedFields);

                    for (int i = 0; i < attribute.DisplayFields.Length; i++)
                    {
                        string fieldName = attribute.DisplayFields[i];

                        var prop = wrappedValueSO.FindProperty(fieldName);

                        if (prop == null)
                        {
                            Debug.LogError($"Could not find property with name {fieldName} on type {wrappedValue.GetType().Name}!");
                            continue;
                        }

                        var propField = new PropertyField(prop);
                        propField.BindProperty(prop);

                        displayFieldsContainer.Add(propField);

                        RemoveChildWithName(fieldName, wrappedFields);
                    }

                    root.AddAll(defaultFields, displayFieldsContainer);

                    if (wrappedFields.childCount > 0)
                    {
                        var wrappedFoldout = createFoldout(wrappedFields);
                        root.Add(wrappedFoldout);
                    }
                }
                else
                {
                    var wrappedFoldout = createFoldout(wrappedFields);

                    root.AddAll(defaultFields, wrappedFoldout);
                }
            }
            else
            {
                var wrappedFoldout = createFoldout(wrappedFields);

                root.AddAll(defaultFields, wrappedFoldout);
            }

            VisualElement createFoldout(VisualElement fields)
            {
                var wrappedFoldout = new Foldout
                {
                    text = $"Wrapped {wrappedValue.GetType().Name} Settings",
                    value = false
                };
                wrappedFoldout.AddStyleSheet(ShearsStyles.InspectorStyles);
                wrappedFoldout.AddToClassList(ShearsStyles.DarkFoldoutClass);

                wrappedFoldout.Add(fields);

                return wrappedFoldout;
            }

            return root;
        }

        public override void OnInspectorGUI()
        {
            VisualElementUtil.CreateDefaultFieldsIMGUI(serializedObject);
        }

        private bool TryGetAttribute(Type type, out CustomWrapperAttribute attribute)
        {
            attribute = type.GetCustomAttribute<CustomWrapperAttribute>();

            return attribute != null;
        }

        private void RemoveChildWithName(string name, VisualElement container)
        {
            VisualElement childToRemove = null;
            foreach (var child in container.Children())
            {
                if (child.name == name)
                {
                    childToRemove = child;
                    break;
                }
            }

            if (childToRemove != null)
                container.Remove(childToRemove);
        }
    }
}
