using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomPropertyDrawer(typeof(Ref<>), true)]
    public sealed class RefEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var valueProp = property.FindPropertyRelative("value");
            var valueField = new PropertyField(valueProp)
            {
                label = $"*{valueProp.displayName}"
            };

            root.Add(valueField);

            return root;
        }
    }
}
