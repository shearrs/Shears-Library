using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomPropertyDrawer(typeof(Option<>), true)]
    public class OptionPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement() { name = "Option Root" };

            var hasValueProp = property.FindPropertyRelative("hasValue");
            var valueProp = property.FindPropertyRelative("value");

            var hasValueField = new PropertyField(hasValueProp)
            {
                label = string.Empty,
                name = "Option Toggle",
            };

            var label = new Label(property.displayName);
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.color = EditorStyles.label.normal.textColor;
            label.style.marginLeft = 4;
            label.style.minWidth = 104.6f;

            var valueField = new PropertyField(valueProp)
            {
                name = "Option Value",
                label = string.Empty,
            };

            bool isGeneric = valueProp.propertyType == SerializedPropertyType.Generic;

            void initializeValueField(GeometryChangedEvent _)
            {
                Label valueLabel = valueField.Query<Label>();
                Toggle toggle = null;

                if (isGeneric)
                    toggle = valueField.Query<Toggle>();

                if (valueLabel == null || (isGeneric && toggle == null))
                    return;

                valueField.UnregisterCallback<GeometryChangedEvent>(initializeValueField);

                valueLabel.text = property.displayName;
                label.style.minWidth = 104.6f + hasValueField.contentRect.width;

                if (isGeneric)
                    toggle.style.marginLeft = 4;
                else
                    InitializeDragging(label, valueField);

                valueField.style.flexGrow = 1;
            }

            valueField.RegisterCallback<GeometryChangedEvent>(initializeValueField);

            if (isGeneric)
            {
                var labelContainer = new VisualElement();
                labelContainer.style.flexDirection = FlexDirection.Row;
                labelContainer.AddAll(hasValueField, label);

                root.AddAll(labelContainer, valueField);
            }
            else
            {
                root.style.flexDirection = FlexDirection.Row;
                root.AddAll(hasValueField, label, valueField);
            }

            return root;
        }

        private void InitializeDragging(Label label, PropertyField valueField)
        {
            IntegerField intField = valueField.Query<IntegerField>();
            FloatField floatField = null;

            if (intField == null)
                floatField = valueField.Query<FloatField>();

            if (floatField == null)
                return;

            label.SetCursor(MouseCursor.ResizeHorizontal);

            const float SENSITIVITY = 0.1f;
            bool isDragging = false;
            var startMousePosition = Vector2.zero;
            int startIntValue = 0;
            float startFloatValue = 0.0f;

            label.RegisterCallback<MouseDownEvent>(onMouseDown);
            label.RegisterCallback<MouseMoveEvent>(onMouseMove);
            label.RegisterCallback<MouseUpEvent>(onMouseUp);

            void onMouseDown(MouseDownEvent evt)
            {
                isDragging = true;
                startMousePosition = evt.localMousePosition;

                if (intField != null)
                    startIntValue = intField.value;
                else
                    startFloatValue = floatField.value;

                label.CaptureMouse();
                evt.StopPropagation();
            }

            void onMouseMove(MouseMoveEvent evt)
            {
                if (!isDragging)
                    return;

                float deltaX = evt.localMousePosition.x - startMousePosition.x;

                if (intField != null)
                {
                    int newValue = startIntValue + Mathf.RoundToInt(deltaX * SENSITIVITY);
                    intField.value = newValue;
                }
                else
                {
                    float newValue = startFloatValue + deltaX * SENSITIVITY;
                    floatField.value = newValue;
                }

                evt.StopPropagation();
            }

            void onMouseUp(MouseUpEvent evt)
            {
                if (!isDragging)
                    return;

                isDragging = false;
                label.ReleaseMouse();
                evt.StopPropagation();
            }
        }
    }
}
