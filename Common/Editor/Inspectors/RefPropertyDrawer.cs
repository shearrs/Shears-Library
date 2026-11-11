using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomPropertyDrawer(typeof(Ref<>), true)]
    public sealed class RefPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var valueProp = property.FindPropertyRelative("value");
            var valueField = new PropertyField(valueProp)
            {
                name = "Ref Property Drawer",
                label = $"*{property.displayName}",
            };

            return valueField;
        }
    }
}
