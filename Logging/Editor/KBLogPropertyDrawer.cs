using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InternProject.Logging.Editor
{
    [CustomPropertyDrawer(typeof(KBLog))]
    public class KBLogPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var messageField = new PropertyField(property.FindPropertyRelative("_message"));
            var contextField = new PropertyField(property.FindPropertyRelative("_context"));
            var logLevelField = CreateLogLevelField(property);
            var usesPrefixField = new PropertyField(property.FindPropertyRelative("_usesCustomPrefix"));
            var prefixField = new PropertyField(property.FindPropertyRelative("_prefix"));
            var usesColorField = new PropertyField(property.FindPropertyRelative("_usesCustomColor"));
            var colorField = new PropertyField(property.FindPropertyRelative("_color"));

            root.Add(messageField);
            root.Add(contextField);
            root.Add(logLevelField);
            root.Add(usesPrefixField);
            root.Add(prefixField);
            root.Add(usesColorField);
            root.Add(colorField);

            return root;
        }

        private VisualElement CreateLogLevelField(SerializedProperty property)
        {
            var logLevelProp = property.FindPropertyRelative("_level");

            var logLevelField = new EnumField("Level", (KBLogLevel)logLevelProp.enumValueFlag);

            logLevelField.RegisterValueChangedCallback(evt =>
            {
                logLevelProp.enumValueFlag = (int)(KBLogLevel)evt.newValue;

                property.serializedObject.ApplyModifiedProperties();
            });

            return logLevelField;
        }
    }
}
