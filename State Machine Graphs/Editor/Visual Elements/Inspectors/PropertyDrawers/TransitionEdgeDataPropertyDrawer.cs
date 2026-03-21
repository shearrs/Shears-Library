using Shears.GraphViews.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    [CustomPropertyDrawer(typeof(TransitionEdgeData))]
    public class TransitionEdgeDataPropertyDrawer : PropertyDrawer
    {
        private readonly List<SerializedProperty> instanceComparisonProps = new();

        public override VisualElement CreatePropertyGUI(SerializedProperty transitionEdgeProp)
        {
            var root = new VisualElement();
            root.AddStyleSheet(SMEditorUtil.SMGraphInspectorStyleSheet);
            root.AddToClassList(SMEditorUtil.TransitionClassName);
            root.Add(CreateTitle(transitionEdgeProp));

            var transitionDataProp = transitionEdgeProp.FindPropertyRelative("transitionData");
            var transitionsContainer = CreateTransitionsContainer();

            if (transitionDataProp.arraySize == 0)
            {
                transitionDataProp.InsertArrayElementAtIndex(0);
                transitionDataProp.GetArrayElementAtIndex(0).boxedValue = new TransitionData();
            }

            for (int i = 0; i < transitionDataProp.arraySize; i++)
            {
                var transitionDataElement = transitionDataProp.GetArrayElementAtIndex(i);
                var comparisonsContainer = CreateComparisonsContainer(i);
                var comparisonList = CreateComparisonList(transitionDataElement);

                comparisonsContainer.Add(CreateAddComparisonButton(transitionDataElement));
                comparisonsContainer.Add(comparisonList);
                transitionsContainer.Add(comparisonsContainer);
            }

            root.Add(transitionsContainer);

            return root;
        }

        private VisualElement CreateTitle(SerializedProperty transitionProp)
        {
            var graphSO = transitionProp.serializedObject;
            var fromID = transitionProp.FindPropertyRelative("fromID").stringValue;
            var toID = transitionProp.FindPropertyRelative("toID").stringValue;

            var fromProp = GraphViewEditorUtil.GetElementProp(graphSO, fromID);
            var toProp = GraphViewEditorUtil.GetElementProp(graphSO, toID);
            var fromNameProp = fromProp.FindPropertyRelative("name");
            var toNameProp = toProp.FindPropertyRelative("name");

            var title = new VisualElement();
            title.AddToClassList(SMEditorUtil.TransitionTitleClassName);

            var fromLabel = new Label();
            var symbolLabel = new Label(" -> ");
            var toLabel = new Label();

            fromLabel.BindProperty(fromNameProp);
            toLabel.BindProperty(toNameProp);

            title.AddAll(fromLabel, symbolLabel, toLabel);

            return title;
        }

        private VisualElement CreateTransitionsContainer()
        {
            var container = new VisualElement();

            container.AddToClassList(SMEditorUtil.TransitionsContainerClassName);

            return container;
        }

        private VisualElement CreateComparisonsContainer(int index)
        {
            var container = new VisualElement();

            container.AddToClassList(SMEditorUtil.ComparisonsContainerClassName);

            var label = new Label($"Transition {index}");
            label.style.marginLeft = 4;

            container.Add(label);

            return container;
        }

        private VisualElement CreateDeleteTransitionButtom(SerializedProperty transitionEdgeProp, SerializedProperty transitionProp)
        {
            var deleteButton = new Button(() => DeleteTransition(transitionEdgeProp, transitionProp))
            {
                text = "X"
            };

            return deleteButton;
        }

        private void DeleteTransition(SerializedProperty transitionEdgeProp, SerializedProperty transitionProp)
        {
            var transitionDataProp = transitionEdgeProp.FindPropertyRelative("transitionData");
            for (int i = 0; i < transitionDataProp.arraySize; i++)
            {
                var transitionData = transitionDataProp.GetArrayElementAtIndex(i);

                if (transitionData == transitionProp)
                    transitionDataProp.DeleteArrayElementAtIndex(i);
            }
        }

        private VisualElement CreateAddComparisonButton(SerializedProperty transitionProp)
        {
            var addComparisonButton = new Button(() => AddComparison(transitionProp))
            {
                text = "+"
            };

            addComparisonButton.AddToClassList(SMEditorUtil.AddComparisonButtonClassName);
            return addComparisonButton;
        }

        private void AddComparison(SerializedProperty transitionProp)
        {
            var comparisonsProp = transitionProp.FindPropertyRelative("comparisonData");
            var size = comparisonsProp.arraySize;
            var comparison = new EmptyComparisonData();

            comparisonsProp.InsertArrayElementAtIndex(size);
            comparisonsProp.GetArrayElementAtIndex(size).boxedValue = comparison;

            comparisonsProp.serializedObject.ApplyModifiedProperties();
        }

        private VisualElement CreateComparisonList(SerializedProperty transitionProp)
        {
            var comparisonsProp = transitionProp.FindPropertyRelative("comparisonData");

            instanceComparisonProps.Clear();

            for (int i = 0; i < comparisonsProp.arraySize; ++i)
                instanceComparisonProps.Add(comparisonsProp.GetArrayElementAtIndex(i));

            var comparisonList = new VisualElement();

            void updateList(SerializedProperty comparisonsProp)
            {
                UpdateComparisonProps(comparisonsProp);
                BuildComparisonList(comparisonList);
            }

            comparisonList.TrackPropertyValue(comparisonsProp, updateList);

            updateList(comparisonsProp);

            return comparisonList;
        }

        private void UpdateComparisonProps(SerializedProperty comparisonsProp)
        {
            instanceComparisonProps.Clear();

            for (int i = 0; i < comparisonsProp.arraySize; ++i)
                instanceComparisonProps.Add(comparisonsProp.GetArrayElementAtIndex(i));
        }

        // TODO: i think if we cache these, the rebuild wont be noticeable
        private void BuildComparisonList(VisualElement comparisonList)
        {
            comparisonList.Clear();

            foreach (var comparison in instanceComparisonProps)
            {
                var comparisonField = new PropertyField();
                comparisonField.BindProperty(comparison);

                comparisonList.Add(comparisonField);
            }
        }
    }
}
