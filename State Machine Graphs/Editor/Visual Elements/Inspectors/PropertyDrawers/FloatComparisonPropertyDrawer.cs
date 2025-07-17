using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    [CustomPropertyDrawer(typeof(FloatComparisonData))]
    public class FloatComparisonPropertyDrawer : ComparisonPropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty comparisonProp)
        {
            var root = new VisualElement();

            root.AddToClassList(SMEditorUtil.ComparisonBodyClassName);
            root.Add(CreateParameterDropdown(comparisonProp));
            root.Add(CreateCompareTypeDropdown(comparisonProp));
            root.Add(CreateFloatField(comparisonProp));
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

        private VisualElement CreateFloatField(SerializedProperty comparisonProp)
        {
            var valueProp = comparisonProp.FindPropertyRelative("compareValue");
            var floatField = new FloatField();
            floatField.labelElement.AddToClassList(SMEditorUtil.ComparisonLabelClassName);
            floatField.AddToClassList(SMEditorUtil.ComparisonIntFieldClassName);

            floatField.BindProperty(valueProp);

            return floatField;
        }
    }
}
