using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomPropertyDrawer(typeof(Placeholder))]
    public class PlaceholderPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                Debug.LogError(
                    $"{nameof(Placeholder)} Attribute can only be applied to string fields."
                );
                return new PropertyField(property);
            }

            var placeholder = attribute as Placeholder;
            var field = new PropertyField(property);
            TextField textField = null;

            void setPlaceholder()
            {
                if (placeholder.PlaceholderMode == Placeholder.Mode.Literal)
                    textField.textEdition.placeholder = placeholder.Value;
                else
                {
                    var parent = property.FindParentProperty();
                    string placeholderValue;

                    if (parent == null)
                        placeholderValue = property.serializedObject.ReflectProperty<string>(
                            placeholder.Value
                        );
                    else
                        placeholderValue = parent.ReflectProperty<string>(placeholder.Value);

                    if (placeholderValue != null)
                        textField.textEdition.placeholder = placeholderValue;
                }
            }

            void initializeField(GeometryChangedEvent _)
            {
                textField = field.Q<TextField>();

                if (textField == null)
                    return;

                setPlaceholder();

                field.UnregisterCallback<GeometryChangedEvent>(initializeField);
            }

            void onValueChanged(SerializedPropertyChangeEvent evt)
            {
                if (textField == null)
                    return;

                if (string.IsNullOrEmpty(evt.changedProperty.stringValue))
                    setPlaceholder();
            }

            field.RegisterCallback<GeometryChangedEvent>(initializeField);
            field.RegisterValueChangeCallback(onValueChanged);

            return field;
        }
    }
}
