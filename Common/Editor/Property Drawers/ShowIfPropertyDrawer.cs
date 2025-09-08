using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

namespace Shears.Editor
{
    /// <summary>
    /// A property drawer for <see cref="ShowIfAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty targetProperty)
        {
            var root = new VisualElement();
            var displayAttribute = attribute as ShowIfAttribute;

            var conditionName = displayAttribute.ConditionName;
            bool negate = conditionName.StartsWith("!");

            if (negate)
                conditionName = conditionName[1..];

            var conditionProperty = GetConditionProperty(targetProperty, conditionName);

            if (conditionProperty == null)
                return root;

            var propertyField = new PropertyField(targetProperty);

            void onValueChanged(SerializedProperty prop)
            {
                bool isEqual = conditionProperty.boxedValue.Equals(displayAttribute.CompareValue);

                if (isEqual != negate)
                    propertyField.style.display = DisplayStyle.Flex;
                else if (root.Children().Contains(propertyField))
                    propertyField.style.display = DisplayStyle.None;
            }

            root.Add(propertyField);
            root.TrackPropertyValue(conditionProperty, onValueChanged);
            onValueChanged(conditionProperty);

            return root;
        }

        public override void OnGUI(Rect position, SerializedProperty targetProperty, GUIContent label)
        {
            var displayAttribute = attribute as ShowIfAttribute;

            var conditionName = displayAttribute.ConditionName;
            bool negate = conditionName.StartsWith("!");

            if (negate)
                conditionName = conditionName[1..];

            var conditionProperty = GetConditionProperty(targetProperty, conditionName);

            if (conditionProperty == null)
                return;

            bool isEqual = conditionProperty.boxedValue.Equals(displayAttribute.CompareValue);

            if (isEqual != negate)
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
