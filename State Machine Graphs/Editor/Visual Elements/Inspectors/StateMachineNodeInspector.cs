using Shears.GraphViews.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class StateMachineNodeInspector : VisualElement
    {
        private readonly StateMachineGraph graphData;
        private readonly List<SerializedProperty> instanceTransitionProps = new();
        private StateMachineNodeData nodeData;
        private SerializedProperty nodeProp;
        private SerializedProperty edgesProp;
        private VisualElement transitionList;

        public StateMachineNodeInspector(StateMachineGraph graphData)
        {
            this.graphData = graphData;

            AddToClassList(SMEditorUtil.StateNodeInspectorClassName);
        }

        public void SetNode(StateMachineNodeData data)
        {
            Clear();

            nodeData = data;
            nodeProp = GraphViewEditorUtil.GetElementProp(graphData, nodeData.ID);
            edgesProp = nodeProp.FindPropertyRelative("edges");

            CreateNameField();
            CreateTransitions();
        }

        private void CreateNameField()
        {
            var nameProp = nodeProp.FindPropertyRelative("name");

            var nameField = new TextField("Name");
            nameField.BindProperty(nameProp);

            Add(nameField);
        }

        private void CreateTransitions()
        {
            var transitionContainer = new VisualElement();
            transitionContainer.AddToClassList(SMEditorUtil.TransitionContainerClassName);
            transitionContainer.TrackPropertyValue(edgesProp, OnEdgesChanged);

            var transitionsTitle = new Label("Transitions");
            transitionsTitle.AddToClassList(SMEditorUtil.TransitionTitleClassName);

            transitionList = new VisualElement();

            UpdateTransitionProps();
            BuildTransitionList();

            transitionContainer.Add(transitionList);
            Add(transitionContainer);
        }

        private void UpdateTransitionProps()
        {
            instanceTransitionProps.Clear();

            for (int i = 0; i < edgesProp.arraySize; i++)
            {
                var edge = edgesProp.GetArrayElementAtIndex(i);
                var transition = GraphViewEditorUtil.GetElementProp(graphData, edge.stringValue);
                var fromIDProp = transition.FindPropertyRelative("fromID");

                if (transition != null && fromIDProp.stringValue == nodeData.ID)
                    instanceTransitionProps.Add(transition);
            }
        }

        private void BuildTransitionList()
        {
            transitionList.Clear();

            foreach (var transition in instanceTransitionProps)
            {
                var transitionField = new PropertyField();
                transitionField.BindProperty(transition);

                transitionList.Add(transitionField);
            }
        }

        private void OnEdgesChanged(SerializedProperty edgesProp)
        {
            UpdateTransitionProps();
            BuildTransitionList();
        }
    }
}
