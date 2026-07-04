using System.Collections.Generic;
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

            var altProps = new List<SerializedProperty>();
            if (reqAttribute.AlternativeValues.Length > 0)
            {
                var parentProp = property.FindParentProperty();

                if (parentProp != null)
                {
                    foreach (var alt in reqAttribute.AlternativeValues)
                        altProps.Add(parentProp.FindPropertyRelative(alt));
                }
                else
                {
                    var parentSO = property.serializedObject;
                    foreach (var alt in reqAttribute.AlternativeValues)
                        altProps.Add(parentSO.FindProperty(alt));
                }
            }

            void initializeField(GeometryChangedEvent evt)
            {
                propertyField.UnregisterCallback<GeometryChangedEvent>(initializeField);
                OnPropertyValueChanged(property, propertyField, altProps, targetSize);
            }

            propertyField.RegisterCallback<GeometryChangedEvent>(initializeField);
            propertyField.RegisterValueChangeCallback(
                (evt) => OnPropertyValueChanged(property, propertyField, altProps, targetSize)
            );

            foreach (var altProp in altProps)
            {
                propertyField.TrackPropertyValue(
                    altProp,
                    (evt) => OnPropertyValueChanged(property, propertyField, altProps, targetSize)
                );
            }

            return root;
        }

        private void OnPropertyValueChanged(
            SerializedProperty property,
            VisualElement field,
            IReadOnlyList<SerializedProperty> altProps,
            int targetSize
        )
        {
            var labels = field.Query<Label>().ToList();
            bool hasValidAltProp = false;

            foreach (var altProp in altProps)
            {
                if (altProp != null && altProp.boxedValue != null)
                {
                    hasValidAltProp = true;
                    break;
                }
            }

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
                else if (property.boxedValue == null && !hasValidAltProp)
                    label.style.color = RED;
                else
                    label.style.color = StyleKeyword.Null;
            }
        }
    }
}
