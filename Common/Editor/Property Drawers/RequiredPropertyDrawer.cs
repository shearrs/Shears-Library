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
            var reqAttribute = attribute as RequiredAttribute;
            var targetSize = reqAttribute.TargetCollectionSize;

            root.Add(propertyField);

            if (
                property.propertyType == SerializedPropertyType.Generic
                && (!property.isArray || targetSize == -1)
            )
                return root;

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
                OnPropertyValueChanged(property, propertyField, altProp, targetSize);
            }

            propertyField.RegisterCallback<GeometryChangedEvent>(initializeField);
            propertyField.RegisterValueChangeCallback(
                (evt) => OnPropertyValueChanged(property, propertyField, altProp, targetSize)
            );

            if (altProp != null)
                propertyField.TrackPropertyValue(
                    altProp,
                    (evt) => OnPropertyValueChanged(property, propertyField, altProp, targetSize)
                );

            return root;
        }

        private void OnPropertyValueChanged(
            SerializedProperty property,
            VisualElement field,
            SerializedProperty altProp,
            int targetSize
        )
        {
            var labels = field.Query<Label>().ToList();
            bool hasAltProp = altProp != null && altProp.boxedValue != null;

            if (property.isArray)
            {
                var label = labels[0];
                labels.Clear();

                labels.Add(label);
            }

            foreach (var label in labels)
            {
                if (property.isArray && targetSize != -1)
                {
                    if (property.arraySize >= targetSize)
                        label.style.color = StyleKeyword.Null;
                    else
                        label.style.color = RED;

                    continue;
                }
                else if (property.boxedValue == null && (!hasAltProp || altProp.boxedValue == null))
                    label.style.color = RED;
                else
                    label.style.color = StyleKeyword.Null;
            }
        }
    }
}
