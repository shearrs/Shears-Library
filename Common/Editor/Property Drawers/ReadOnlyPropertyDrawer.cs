using Shears;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var field = new PropertyField(property);
            field.SetEnabled(false);

            root.Add(field);

            return root;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool previousGUIState = GUI.enabled;

            GUI.enabled = false;

            EditorGUI.PropertyField(position, property, label);

            GUI.enabled = previousGUIState;
        }
    }
}