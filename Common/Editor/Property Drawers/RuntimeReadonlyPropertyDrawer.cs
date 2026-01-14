using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomPropertyDrawer(typeof(RuntimeReadOnlyAttribute))]
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

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool previousGUIState = GUI.enabled;

            if (Application.isPlaying)
                GUI.enabled = false;

            EditorGUI.PropertyField(position, property, label);

            GUI.enabled = previousGUIState;
        }
    }
}
