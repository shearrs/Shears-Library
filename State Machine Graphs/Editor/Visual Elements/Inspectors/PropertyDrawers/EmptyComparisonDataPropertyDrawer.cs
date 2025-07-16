using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Shears.Editor;

namespace Shears.StateMachineGraphs.Editor
{
    [CustomPropertyDrawer(typeof(EmptyComparisonData))]
    public class EmptyComparisonDataPropertyDrawer : PropertyDrawer
    {
        private readonly List<string> instanceParameterNames = new();

        public override VisualElement CreatePropertyGUI(SerializedProperty comparisonProp)
        {
            var root = new VisualElement();

            root.AddToClassList(SMEditorUtil.ComparisonBodyClassName);
            root.Add(CreateDropdownField(comparisonProp));
            root.Add(CreateRemoveButton(comparisonProp));

            return root;
        }

        private VisualElement CreateDropdownField(SerializedProperty comparisonProp)
        {
            var graphSO = comparisonProp.serializedObject;
            var graphData = graphSO.targetObject as StateMachineGraph;

            var parameters = graphData.GetParameters();
            instanceParameterNames.Clear();

            foreach (var parameter in parameters)
            {
                instanceParameterNames.Add(parameter.Name);
            }

            var dropdown = new DropdownField(instanceParameterNames, -1);
            dropdown.AddToClassList(SMEditorUtil.ComparisonDropdownClassName);
            dropdown.RegisterValueChangedCallback(OnDropdownChanged);

            return dropdown;
        }

        private VisualElement CreateRemoveButton(SerializedProperty comparisonProp)
        {
            var button = new Button(() => RemoveComparison(comparisonProp))
            {
                text = "X"
            };

            button.AddToClassList(SMEditorUtil.AddComparisonButtonClassName);

            return button;
        }

        private void RemoveComparison(SerializedProperty comparisonProp)
        {
            var comparisonsProp = comparisonProp.FindParentProperty();
            int index = -1;
            
            for (int i = 0; i < comparisonsProp.arraySize; i++)
            {
                var element = comparisonsProp.GetArrayElementAtIndex(i);

                if (SerializedProperty.EqualContents(element, comparisonProp))
                {
                    index = i;
                    break;
                }
            }

            comparisonsProp.DeleteArrayElementAtIndex(index);
            comparisonsProp.serializedObject.ApplyModifiedProperties();
        }

        private void OnDropdownChanged(ChangeEvent<string> evt)
        {
            Debug.Log(evt.newValue);
        }
    }
}
