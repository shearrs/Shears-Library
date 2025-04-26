using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.UI.Editor
{
    [CustomPropertyDrawer(typeof(ManagedUINavigation))]
    public class ManagedUINavigationPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var typeProp = property.FindPropertyRelative("type");
            var type = (ManagedUINavigation.NavigationType)typeProp.enumValueIndex;

            var typeField = new PropertyField(typeProp);
            var upField = new PropertyField(property.FindPropertyRelative("up"));
            var rightField = new PropertyField(property.FindPropertyRelative("right"));
            var downField = new PropertyField(property.FindPropertyRelative("down"));
            var leftField = new PropertyField(property.FindPropertyRelative("left"));

            IncludeOnFlag(!IsTypeAutomatic(type), root, upField, rightField, downField, leftField);

            typeField.RegisterValueChangeCallback(evt =>
            {
                var type = (ManagedUINavigation.NavigationType)typeProp.enumValueIndex;

                bool isAutomatic = IsTypeAutomatic(type);

                IncludeOnFlag(!isAutomatic, root, upField, rightField, downField, leftField);
            });

            typeField.style.paddingBottom = 8;

            root.Add(typeField);
            root.Add(upField);
            root.Add(rightField);
            root.Add(downField);
            root.Add(leftField);

            return root;
        }

        private void IncludeOnFlag(bool flag, VisualElement parent, params VisualElement[] elements)
        {
            foreach (var element in elements)
            {
                if (flag)
                    parent.Add(element);
                else
                    parent.Remove(element);
            }
        }

        private bool IsTypeAutomatic(ManagedUINavigation.NavigationType type)
        {
            return type == ManagedUINavigation.NavigationType.AutomaticStatic || type == ManagedUINavigation.NavigationType.AutomaticDynamic;
        }
    }
}
