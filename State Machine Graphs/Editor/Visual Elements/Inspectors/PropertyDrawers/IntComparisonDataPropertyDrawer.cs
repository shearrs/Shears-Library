using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    [CustomPropertyDrawer(typeof(IntComparisonData))]
    public class IntComparisonDataPropertyDrawer : ComparisonPropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty comparisonProp)
        {
            var root = new VisualElement();

            root.AddToClassList(SMEditorUtil.ComparisonBodyClassName);
            root.Add(CreateParameterDropdown(comparisonProp));
            root.Add(CreateCompareTypeDropdown(comparisonProp));
            root.Add(CreateIntField(comparisonProp));
            root.Add(CreateRemoveButton(comparisonProp));

            return root;
        }

        private VisualElement CreateCompareTypeDropdown(SerializedProperty comparisonProp)
        {
            var compareTypeProp = comparisonProp.FindPropertyRelative("compareType");
            var dropdown = new DropdownField();
            dropdown.BindProperty(compareTypeProp);

            dropdown.AddToClassList(SMEditorUtil.CompareTypeDropdownClassName);

            return dropdown;
        }

        private VisualElement CreateIntField(SerializedProperty comparisonProp)
        {
            var valueProp = comparisonProp.FindPropertyRelative("compareValue");
            var intField = new IntegerField();
            intField.labelElement.AddToClassList(SMEditorUtil.ComparisonLabelClassName);
            intField.AddToClassList(SMEditorUtil.ComparisonIntFieldClassName);

            intField.BindProperty(valueProp);

            return intField;
        }
    }
}
