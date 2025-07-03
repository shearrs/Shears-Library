using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachines.Editor
{
    [CustomPropertyDrawer(typeof(FloatParameterComparison), true)]
    public class FloatParameterComparisonPropertyDrawer : ParameterComparisonPropertyDrawer
    {
        protected override VisualElement CreateCompareValueField(SerializedProperty property)
        {
            var compareTypeField = new PropertyField(property.FindPropertyRelative("compareValueType"));
            var compareValueField = new PropertyField(property.FindPropertyRelative("compareValue"));

            var container = new VisualElement();
            container.Add(compareTypeField);
            container.Add(compareValueField);

            return container;
        }
    }
}
