using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Shears.Editor
{
    /// <summary>
    /// A property drawer for <see cref="ShowIfAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfPropertyDrawer : PropertyDrawer
    {
        private readonly List<Comparison> comparisons = new();

        private readonly struct Comparison
        {
            private readonly string propertyName;
            private readonly bool negate;
            private readonly object compareValue;

            public Comparison(string propertyName, bool negate, object compareValue)
            {
                this.propertyName = propertyName;
                this.negate = negate;
                this.compareValue = compareValue;
            }

            public readonly bool Evaluate(SerializedProperty parent)
            {
                var property = parent.FindPropertyRelative(propertyName);
                Debug.Log($"does {property.boxedValue} == {compareValue}");

                return property.boxedValue.Equals(compareValue) != negate;
            }

            public readonly bool Evaluate(SerializedObject parent)
            {
                var property = parent.FindProperty(propertyName);
                return property.boxedValue.Equals(compareValue) != negate;
            }
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty targetProperty)
        {
            var root = new VisualElement();
            var showIfAttribute = attribute as ShowIfAttribute;

            comparisons.Clear();

            foreach (var conditionName in showIfAttribute.Conditions.Keys)
            {
                string name = conditionName;
                bool negate = name.StartsWith("!");

                if (negate)
                    name = conditionName[1..];

                var conditionProperty = GetConditionProperty(targetProperty, name);

                if (conditionProperty == null)
                    return root;

                comparisons.Add(new(name, negate, showIfAttribute.Conditions[conditionName]));
            }

            var propertyField = new PropertyField(targetProperty)
            {
                name = targetProperty.displayName
            };

            void onValueChanged(SerializedProperty parentProperty, SerializedObject parentObject)
            {
                bool isValid = true;

                foreach (var comparison in comparisons)
                {
                    if (parentProperty != null)
                    {
                        if (!comparison.Evaluate(parentProperty))
                        {
                            isValid = false;
                            break;
                        }
                    }
                    else
                    {
                        if (!comparison.Evaluate(parentObject))
                        {
                            isValid = false;
                            break;
                        }
                    }
                }

                if (isValid)
                    propertyField.style.display = DisplayStyle.Flex;
                else if (root.Children().Contains(propertyField))
                    propertyField.style.display = DisplayStyle.None;
            }

            root.Add(propertyField);

            var parentProperty = targetProperty.FindParentProperty();

            if (parentProperty != null)
                root.TrackPropertyValue(parentProperty, _ => onValueChanged(parentProperty, targetProperty.serializedObject));
            else
                root.TrackSerializedObjectValue(targetProperty.serializedObject, _ => onValueChanged(parentProperty, targetProperty.serializedObject));

            onValueChanged(parentProperty, targetProperty.serializedObject);

            return root;
        }

        public override void OnGUI(Rect position, SerializedProperty targetProperty, GUIContent label)
        {
            var displayAttribute = attribute as ShowIfAttribute;

            comparisons.Clear();

            foreach (var conditionName in displayAttribute.Conditions.Keys)
            {
                string name = conditionName;
                bool negate = name.StartsWith("!");

                if (negate)
                    name = conditionName[1..];

                var conditionProperty = GetConditionProperty(targetProperty, name);

                if (conditionProperty == null)
                    return;

                comparisons.Add(new(name, negate, displayAttribute.Conditions[conditionName]));
            }

            bool isValid = true;
            var parentProperty = targetProperty.FindParentProperty();

            foreach (var comparison in comparisons)
            {
                if (parentProperty != null)
                {
                    if (!comparison.Evaluate(parentProperty))
                    {
                        isValid = false;
                        break;
                    }
                }
                else
                {
                    if (!comparison.Evaluate(targetProperty.serializedObject))
                    {
                        isValid = false;
                        break;
                    }
                }
            }

            if (isValid)
                EditorGUI.PropertyField(position, targetProperty, label);
        }

        private SerializedProperty GetConditionProperty(SerializedProperty targetProperty, string conditionName)
        {
            var parent = targetProperty.FindParentProperty();

            if (parent != null)
                return parent.FindPropertyRelative(conditionName);

            var serializedObject = targetProperty.serializedObject;

            return serializedObject.FindProperty(conditionName);
        }
    }
}
