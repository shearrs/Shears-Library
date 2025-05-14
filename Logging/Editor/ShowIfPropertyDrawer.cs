using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace InternProject.Logging.Editor
{
    /// <summary>
    /// A property drawer for <see cref="ShowIfAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var displayAttribute = attribute as ShowIfAttribute;

            var conditionField = fieldInfo.DeclaringType.GetField(displayAttribute.ConditionName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            
            if (conditionField == null)
            {
                Debug.Log($"Condition {displayAttribute.ConditionName} doesn't exist!");
                return;
            }

            SerializedProperty parent = property.FindParentProperty();
            object target = parent == null ? property.serializedObject.targetObject : parent.boxedValue;

            if (conditionField.GetValue(target).Equals(displayAttribute.CompareValue))
                EditorGUI.PropertyField(position, property, label);
        }
    }
}
