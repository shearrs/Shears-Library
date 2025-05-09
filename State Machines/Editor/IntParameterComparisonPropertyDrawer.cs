using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InternProject.StateMachines.Editor
{
    [CustomPropertyDrawer(typeof(IntParameterComparison), true)]
    public class IntParameterComparisonPropertyDrawer : ParameterComparisonPropertyDrawer
    {
        protected override VisualElement CreateCompareValueField(SerializedProperty property)
        {
            var compareTypeField = new PropertyField(property.FindPropertyRelative("_compareValueType"));
            var compareValueField = new PropertyField(property.FindPropertyRelative("_compareValue"));

            var container = new VisualElement();
            container.Add(compareTypeField);
            container.Add(compareValueField);

            return container;
        }
    }
}
