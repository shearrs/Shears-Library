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

            var keyField = new PropertyField(keyProp);
            var valueField = new PropertyField(valueProp);

            root.AddAll(keyField, valueField);

            return root;
        }
    }
}
