using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomPropertyDrawer(typeof(RuntimeReadonlyAttribute))]
    public class RuntimeReadonlyPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var field = new PropertyField(property);

            if (Application.isPlaying)
                field.SetEnabled(false);

            root.Add(field);

            return root;
        }
    }
}
