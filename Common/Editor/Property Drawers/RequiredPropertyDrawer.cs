using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredPropertyDrawer : PropertyDrawer
    {
        private static readonly Color RED = new(1.0f, 0.2f, 0.2f);

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement { name = "Required Drawer" };
            var propertyField = new PropertyField(property) { name = property.displayName };

            root.Add(propertyField);

            if (property.propertyType == SerializedPropertyType.Generic)
                return root;

            var reqAttribute = attribute as RequiredAttribute;

            SerializedProperty altProp = null;
            if (reqAttribute.AlternativeValue != null)
            {
                var parentProp = property.FindParentProperty();

                if (parentProp != null)
                    altProp = parentProp.FindPropertyRelative(reqAttribute.AlternativeValue);
                else
                {
                    var parentSO = property.serializedObject;
                    altProp = parentSO.FindProperty(reqAttribute.AlternativeValue);
                }
            }

            void initializeField(GeometryChangedEvent evt)
            {
                propertyField.UnregisterCallback<GeometryChangedEvent>(initializeField);
                OnPropertyValueChanged(property, propertyField, altProp);
            }

            propertyField.RegisterCallback<GeometryChangedEvent>(initializeField);
            propertyField.RegisterValueChangeCallback(
                (evt) => OnPropertyValueChanged(property, propertyField, altProp)
            );

            if (altProp != null)
                propertyField.TrackPropertyValue(
                    altProp,
                    (evt) => OnPropertyValueChanged(property, propertyField, altProp)
                );

            return root;
        }

        private void OnPropertyValueChanged(
            SerializedProperty property,
            VisualElement field,
            SerializedProperty altProp
        )
        {
            var labels = field.Query<Label>().ToList();
            bool hasAltProp = altProp != null && altProp.boxedValue != null;

            foreach (var label in labels)
            {
                if (property.boxedValue == null && !hasAltProp)
                    label.style.color = RED;
                else
                    label.style.color = StyleKeyword.Null;
            }
        }
    }
}
