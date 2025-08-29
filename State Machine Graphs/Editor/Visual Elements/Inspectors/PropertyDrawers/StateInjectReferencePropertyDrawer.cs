using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    [CustomPropertyDrawer(typeof(StateInjectReference))]
    public class StateInjectReferencePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            var reference = property.boxedValue as StateInjectReference;

            var valueField = new ObjectField
            {
                objectType = reference.FieldType
            };

            valueField.BindProperty(property.FindPropertyRelative("value"));

            root.Add(valueField);

            return root;
        }
    }
}
