using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachines.Editor
{
    [CustomPropertyDrawer(typeof(TriggerParameterComparison))]
    public class TriggerParameterComparisonPropertyDrawer : ParameterComparisonPropertyDrawer
    {
        protected override VisualElement CreateCompareValueField(SerializedProperty property)
        {
            return null;
        }
    }
}
