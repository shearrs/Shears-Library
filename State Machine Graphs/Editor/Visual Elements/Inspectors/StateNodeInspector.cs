using Shears.GraphViews.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class StateNodeInspector : VisualElement
    {
        private readonly StateMachineGraph graphData;
        private readonly List<SerializedProperty> instanceTransitionProps = new();
        private StateNodeData nodeData;
        private SerializedProperty nodeProp;
        private SerializedProperty edgesProp;
        private VisualElement transitionList;

        public StateNodeInspector(StateMachineGraph graphData)
        {
            this.graphData = graphData;

            AddToClassList(SMEditorUtil.StateNodeInspectorClassName);
        }

        public void SetNode(StateNodeData data)
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

                if (transition != null)
                    instanceTransitionProps.Add(GraphViewEditorUtil.GetElementProp(graphData, edge.stringValue));
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
