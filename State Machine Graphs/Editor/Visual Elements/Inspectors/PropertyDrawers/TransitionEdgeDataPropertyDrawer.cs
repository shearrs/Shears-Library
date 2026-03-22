using Shears.GraphViews;
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

            var transitionsContainer = CreateTransitionsContainer();
            var transitionsList = CreateTransitionsList(transitionEdgeProp);

            transitionsContainer.Add(transitionsList);

            root.Add(transitionsContainer);

            return root;
        }

        private VisualElement CreateTitle(SerializedProperty transitionEdgeProp)
        {
            var graphSO = transitionEdgeProp.serializedObject;
            var fromID = transitionEdgeProp.FindPropertyRelative("fromID").stringValue;
            var toID = transitionEdgeProp.FindPropertyRelative("toID").stringValue;

            var fromProp = GraphViewEditorUtil.GetElementProp(graphSO, fromID);
            var toProp = GraphViewEditorUtil.GetElementProp(graphSO, toID);
            var fromNameProp = fromProp.FindPropertyRelative("name");
            var toNameProp = toProp.FindPropertyRelative("name");

            var title = new VisualElement();
            title.AddToClassList(SMEditorUtil.TransitionTitleClassName);

            var addTransitionButton = new Button(() => AddTransition(transitionEdgeProp))
            {
                text = "+"
            };
            addTransitionButton.AddToClassList(SMEditorUtil.AddTransitionClassName);

            var labelContainer = new VisualElement();
            var fromLabel = new Label();
            var symbolLabel = new Label(" -> ");
            var toLabel = new Label();

            labelContainer.style.flexDirection = FlexDirection.Row;
            labelContainer.style.alignItems = Align.Center;
            labelContainer.style.marginLeft = StyleKeyword.Auto;
            labelContainer.style.marginRight = StyleKeyword.Auto;

            labelContainer.AddAll(fromLabel, symbolLabel, toLabel);

            fromLabel.BindProperty(fromNameProp);
            toLabel.BindProperty(toNameProp);

            title.AddAll(labelContainer, addTransitionButton);

            return title;
        }

        private VisualElement CreateTransitionsContainer()
        {
            var container = new VisualElement();

            container.AddToClassList(SMEditorUtil.TransitionsContainerClassName);

            return container;
        }

        private VisualElement CreateComparisonsContainer(SerializedProperty transitionEdgeProp, SerializedProperty transitionDataProp, int index)
        {
            var container = new VisualElement();
            var labelContainer = new VisualElement();
            var comparisonButton = CreateAddComparisonButton(transitionDataProp);
            var deleteButton = CreateDeleteTransitionButtom(transitionEdgeProp, transitionDataProp);

            comparisonButton.style.marginLeft = StyleKeyword.Auto;
            labelContainer.style.flexDirection = FlexDirection.Row;

            container.AddToClassList(SMEditorUtil.ComparisonsContainerClassName);

            var label = new Label($"Transition {index}");
            label.style.marginLeft = 4;

            labelContainer.AddAll(label, comparisonButton, deleteButton);
            container.Add(labelContainer);

            return container;
        }

        private VisualElement CreateTransitionsList(SerializedProperty transitionEdgeProp)
        {
            var transitionDataProp = transitionEdgeProp.FindPropertyRelative("transitionData");
            var transitionsList = new VisualElement();

            if (transitionDataProp.arraySize == 0)
            {
                transitionDataProp.InsertArrayElementAtIndex(0);
                transitionDataProp.GetArrayElementAtIndex(0).boxedValue = new TransitionData();
            }
            else if (transitionDataProp.GetArrayElementAtIndex(0).boxedValue == null)
                transitionDataProp.GetArrayElementAtIndex(0).boxedValue = new TransitionData();

            transitionDataProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            transitionDataProp.serializedObject.Update();

            void updateList(SerializedProperty comparisonsProp)
            {
                transitionsList.Clear();

                for (int i = 0; i < transitionDataProp.arraySize; i++)
                {
                    var transitionDataElement = transitionDataProp.GetArrayElementAtIndex(i);
                    var comparisonsContainer = CreateComparisonsContainer(transitionEdgeProp, transitionDataElement, i);
                    var comparisonList = CreateComparisonList(transitionDataElement);

                    comparisonsContainer.Add(comparisonList);
                    transitionsList.Add(comparisonsContainer);
                }
            }

            transitionsList.TrackPropertyValue(transitionDataProp, updateList);
            updateList(transitionDataProp);

            return transitionsList;
        }

        private VisualElement CreateDeleteTransitionButtom(SerializedProperty transitionEdgeProp, SerializedProperty transitionDataProp)
        {
            var deleteButton = new Button(() => DeleteTransition(transitionEdgeProp, transitionDataProp))
            {
                text = "X"
            };

            return deleteButton;
        }

        private void AddTransition(SerializedProperty transitionEdgeProp)
        {
            var transitionDataProp = transitionEdgeProp.FindPropertyRelative("transitionData");
            var size = transitionDataProp.arraySize;
            var transition = new TransitionData();

            transitionDataProp.InsertArrayElementAtIndex(size);
            transitionDataProp.GetArrayElementAtIndex(size).boxedValue = transition;

            transitionDataProp.serializedObject.ApplyModifiedProperties();
        }

        private void DeleteTransition(SerializedProperty transitionEdgeProp, SerializedProperty targetTransitionProp)
        {
            var transitionDataProp = transitionEdgeProp.FindPropertyRelative("transitionData");
            for (int i = 0; i < transitionDataProp.arraySize; i++)
            {
                var transitionData = transitionDataProp.GetArrayElementAtIndex(i);

                if (SerializedProperty.DataEquals(transitionData, targetTransitionProp))
                    transitionDataProp.DeleteArrayElementAtIndex(i);
            }

            transitionDataProp.serializedObject.ApplyModifiedProperties();
            transitionDataProp.serializedObject.Update();

            if (transitionDataProp.arraySize == 0)
            {
                // get serializedObject then remove transition edge data
                var edgeData = transitionEdgeProp.boxedValue as TransitionEdgeData;
                var graphData = transitionEdgeProp.serializedObject.targetObject as GraphData;

                // get the graph view so we can save
                SMEditorWindow.GraphView.Record("Delete Transition");
                graphData.Editor__RemoveEdgeData(edgeData);
                SMEditorWindow.GraphView.Save();

                SMEditorWindow.GraphView.Select(null);
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
                instanceComparisonProps.Clear();

                for (int i = 0; i < comparisonsProp.arraySize; ++i)
                    instanceComparisonProps.Add(comparisonsProp.GetArrayElementAtIndex(i));

                comparisonList.Clear();

                var label = new Label("Comparisons");
                label.style.alignSelf = Align.Center;

                comparisonList.Add(label);

                foreach (var comparison in instanceComparisonProps)
                {
                    var comparisonField = new PropertyField();
                    comparisonField.BindProperty(comparison);

                    comparisonList.Add(comparisonField);
                }
            }

            comparisonList.TrackPropertyValue(comparisonsProp, updateList);
            updateList(comparisonsProp);

            return comparisonList;
        }
    }
}
