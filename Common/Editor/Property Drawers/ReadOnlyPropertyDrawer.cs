using Shears;
using UnityEditor;
using UnityEngine;

namespace Shears.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool previousGUIState = GUI.enabled;

            GUI.enabled = false;

            EditorGUI.PropertyField(position, property, label);

            GUI.enabled = previousGUIState;
        }
    }
}