using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    [CustomPropertyDrawer(typeof(BoolComparisonData))]
    public class BoolComparisonDataPropertyDrawer : ComparisonPropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty comparisonProp)
        {
            var root = new VisualElement();

            root.AddToClassList(SMEditorUtil.ComparisonBodyClassName);
            root.Add(CreateParameterDropdown(comparisonProp));
            root.Add(CreateToggle(comparisonProp));
            root.Add(CreateRemoveButton(comparisonProp));

            return root;
        }

        private VisualElement CreateToggle(SerializedProperty comparisonProp)
        {
            var valueProp = comparisonProp.FindPropertyRelative("compareValue");
            var toggle = new Toggle();
            toggle.labelElement.AddToClassList(SMEditorUtil.ComparisonLabelClassName);
            toggle.AddToClassList(SMEditorUtil.ComparisonToggleClassName);

            toggle.BindProperty(valueProp);

            return toggle;
        }
    }
}
