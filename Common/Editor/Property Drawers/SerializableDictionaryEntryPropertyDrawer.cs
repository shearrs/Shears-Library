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
            root.style.paddingLeft = 16;
            root.style.paddingRight = 4;

            var keyProp = property.FindPropertyRelative("key");
            var valueProp = property.FindPropertyRelative("value");

            var keyField = new PropertyField(keyProp);
            var valueField = new PropertyField(valueProp);

            root.Add(keyField);
            root.Add(valueField);

            return root;
        }
    }
}
