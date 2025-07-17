using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Shears.Editor;

namespace Shears.StateMachineGraphs.Editor
{
    [CustomPropertyDrawer(typeof(EmptyComparisonData))]
    public class EmptyComparisonDataPropertyDrawer : ComparisonPropertyDrawer
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
