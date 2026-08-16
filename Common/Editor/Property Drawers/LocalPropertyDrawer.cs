using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomPropertyDrawer(typeof(LocalAttribute))]
    public class LocalPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var field = new PropertyField(property);
            var target = property.serializedObject.targetObject as Component;

            if (PrefabUtility.IsAnyPrefabInstanceRoot(target.gameObject))
                field.style.display = DisplayStyle.None;

            return field;
        }
    }
}
