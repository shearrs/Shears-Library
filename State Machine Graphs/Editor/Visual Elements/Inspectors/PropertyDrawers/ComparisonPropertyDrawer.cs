using Shears.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    [CustomPropertyDrawer(typeof(ParameterComparisonData))]
    public abstract class ComparisonPropertyDrawer : PropertyDrawer
    {
        private readonly List<string> instanceParameterNames = new();

        protected VisualElement CreateDropdownField(SerializedProperty comparisonProp)
        {
            var graphSO = comparisonProp.serializedObject;
            var graphData = graphSO.targetObject as StateMachineGraph;
            var parameters = graphData.GetParameters();
            var parameterIDProp = comparisonProp.FindPropertyRelative("parameterID");
            var parameterIndex = -1;

            instanceParameterNames.Clear();

            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].ID == parameterIDProp.stringValue)
                    parameterIndex = i;

                instanceParameterNames.Add(parameters[i].Name);
            }

            var dropdown = new DropdownField(instanceParameterNames, parameterIndex);
            dropdown.AddToClassList(SMEditorUtil.ComparisonDropdownClassName);
            dropdown.RegisterValueChangedCallback((evt) => OnDropdownChanged(comparisonProp, evt.newValue));

            return dropdown;
        }

        protected VisualElement CreateRemoveButton(SerializedProperty comparisonProp)
        {
            var button = new Button(() => RemoveComparison(comparisonProp))
            {
                text = "X"
            };

            button.AddToClassList(SMEditorUtil.RemoveComparisonButtonClassName);

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

        private void OnDropdownChanged(SerializedProperty comparisonProp, string selectedParameterName)
        {
            var graphSO = comparisonProp.serializedObject;
            var graphData = graphSO.targetObject as StateMachineGraph;
            var parameters = graphData.GetParameters();
            ParameterData selectedParameter = null;

            foreach (var parameter in parameters)
            {
                if (parameter.Name == selectedParameterName)
                {
                    selectedParameter = parameter;
                    break;
                }
            }

            if (selectedParameter == null)
            {
                Debug.LogError("Could not find parameter with name: " + selectedParameterName);
                return;
            }

            comparisonProp.boxedValue = selectedParameter.CreateComparison();
            graphSO.ApplyModifiedProperties();
        }
    }
}
