using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDictionaryEntry<,>), true)]
    public class SerializableDictionaryEntryPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.SetAllPadding(0, 4, 0, 16);

            var keyProp = property.FindPropertyRelative("key");
            var valueProp = property.FindPropertyRelative("value");

            var keyField = CreateExplicitField(keyProp);

            var valueField = new PropertyField(valueProp);

            root.AddAll(keyField, valueField);

            return root;
        }

        private VisualElement CreateExplicitField(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Float:
                    var floatField = new FloatField(prop.displayName) { isDelayed = true };
                    floatField.BindProperty(prop);
                    return floatField;

                case SerializedPropertyType.Integer:
                    var intField = new IntegerField(prop.displayName) { isDelayed = true };
                    intField.BindProperty(prop);
                    return intField;

                case SerializedPropertyType.String:
                    var textField = new TextField(prop.displayName) { isDelayed = true };
                    textField.BindProperty(prop);
                    return textField;

                default:
                    // Fallback for Objects, Vectors, Enums, and everything else
                    var defaultField = new PropertyField(prop);
                    defaultField.BindProperty(prop);
                    return defaultField;
            }
        }
    }
}
