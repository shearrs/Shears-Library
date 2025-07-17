using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    [CustomPropertyDrawer(typeof(TriggerComparisonData))]
    public class TriggerComparisonPropertyDrawer : ComparisonPropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty comparisonProp)
        {
            var root = new VisualElement();

            root.AddToClassList(SMEditorUtil.ComparisonBodyClassName);
            root.Add(CreateParameterDropdown(comparisonProp));
            root.Add(CreateRemoveButton(comparisonProp));

            return root;
        }
    }
}
