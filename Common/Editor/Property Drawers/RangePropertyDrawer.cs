using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomPropertyDrawer(typeof(Range<>), true)]
    public class RangePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.AddStyleSheet(ShearsStyles.InspectorStyles);
            root.AddToClassList(ShearsStyles.DarkContainerClass);

            root.style.marginTop = 2;
            root.style.marginBottom = 2;

            var minProp = property.FindPropertyRelative("min");
            var maxProp = property.FindPropertyRelative("max");

            string title = property.displayName;

            var header = VisualElementUtil.CreateHeader(title);

            var minField = new PropertyField(minProp);
            var maxField = new PropertyField(maxProp);

            header.style.marginTop = 2;

            root.AddAll(header, minField, maxField);

            void enforceMinMax(SerializedPropertyChangeEvent evt)
            {
                if (minProp.boxedValue is IComparable minValue && maxProp.boxedValue is IComparable maxValue)
                {
                    if (minValue.CompareTo(maxValue) > 0)
                    {
                        if (SerializedProperty.EqualContents(evt.changedProperty, minProp))
                            maxProp.boxedValue = minValue;
                        else
                            minProp.boxedValue = maxValue;

                        minProp.serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            minField.RegisterValueChangeCallback(enforceMinMax);
            maxField.RegisterValueChangeCallback(enforceMinMax);

            return root;
        }
    }
}
