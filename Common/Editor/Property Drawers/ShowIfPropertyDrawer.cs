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

            var parent = targetProperty.FindParentProperty();
            var conditionProperty = parent.FindPropertyRelative(displayAttribute.ConditionName);
            
            if (conditionProperty == null)
                return root;

            var propertyField = new PropertyField(targetProperty);

            void onValueChanged(SerializedProperty prop)
            {
                if (conditionProperty.boxedValue.Equals(displayAttribute.CompareValue))
                    root.Add(propertyField);
                else if (root.Children().Contains(propertyField))
                    root.Remove(propertyField);
            }

            root.TrackPropertyValue(conditionProperty, onValueChanged);
            onValueChanged(conditionProperty);

            return root;
        }
    }
}
