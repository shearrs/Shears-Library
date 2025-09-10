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
            private readonly SerializedProperty property;
            private readonly bool negate;
            private readonly object compareValue;

            public Comparison(SerializedProperty property, bool negate, object compareValue)
            {
                this.property = property;
                this.negate = negate;
                this.compareValue = compareValue;
            }

            public bool Evaluate()
            {
                return property.boxedValue.Equals(compareValue) != negate;
            }
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty targetProperty)
        {
            var root = new VisualElement();
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
                    return root;

                comparisons.Add(new(conditionProperty, negate, displayAttribute.Conditions[conditionName]));
            }

            var propertyField = new PropertyField(targetProperty);

            void onValueChanged(SerializedObject serializedObject)
            {
                bool isValid = true;

                foreach (var comparison in comparisons)
                {
                    if (!comparison.Evaluate())
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                    propertyField.style.display = DisplayStyle.Flex;
                else if (root.Children().Contains(propertyField))
                    propertyField.style.display = DisplayStyle.None;
            }

            root.Add(propertyField);
            root.TrackSerializedObjectValue(targetProperty.serializedObject, onValueChanged);
            onValueChanged(targetProperty.serializedObject);

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

                comparisons.Add(new(conditionProperty, negate, displayAttribute.Conditions[conditionName]));
            }

            bool isValid = true;

            foreach (var comparison in comparisons)
            {
                if (!comparison.Evaluate())
                {
                    isValid = false;
                    break;
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
