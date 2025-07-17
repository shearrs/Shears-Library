using Shears.GraphViews.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    [CustomPropertyDrawer(typeof(TransitionEdgeData))]
    public class TransitionEdgeDataPropertyDrawer : PropertyDrawer
    {
        private readonly List<SerializedProperty> instanceComparisonProps = new();

        public override VisualElement CreatePropertyGUI(SerializedProperty transitionProp)
        {
            var root = new VisualElement();
            root.AddStyleSheet(SMEditorUtil.SMGraphInspectorStyleSheet);
            root.AddToClassList(SMEditorUtil.TransitionClassName);

            root.Add(CreateTitle(transitionProp));

            var comparisonsContainer = CreateComparisonsContainer();
            var comparisonList = CreateComparisonList(transitionProp);
            root.Add(CreateAddComparisonButton(comparisonList, transitionProp));
            comparisonsContainer.Add(comparisonList);

            root.Add(comparisonsContainer);

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

        private VisualElement CreateComparisonsContainer()
        {
            var container = new VisualElement();

            container.AddToClassList(SMEditorUtil.ComparisonsContainerClassName);

            return container;
        }

        private VisualElement CreateAddComparisonButton(VisualElement comparisonList, SerializedProperty transitionProp)
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
