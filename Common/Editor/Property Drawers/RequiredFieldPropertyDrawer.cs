using UnityEditor;
using UnityEngine;

namespace Shears.Editor
{
    [CustomPropertyDrawer(typeof(RequiredFieldAttribute))]
    public class RequiredFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RequiredFieldAttribute requiredField = attribute as RequiredFieldAttribute;

            if (property.objectReferenceValue == null)
            {
                GUI.color = requiredField.Color;
                EditorGUI.PropertyField(position, property, label);
                GUI.color = Color.white;
            }
            else
                EditorGUI.PropertyField(position, property, label);
        }
    }
}
