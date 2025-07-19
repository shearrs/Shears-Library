using Shears.GraphViews;
using Shears.GraphViews.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public abstract class GraphNodeInspector<NodeDataType> : VisualElement where NodeDataType : GraphNodeData
    {
        protected readonly GraphData graphData;
        private readonly List<SerializedProperty> instanceTransitionProps = new();
        protected NodeDataType nodeData;
        protected SerializedProperty nodeProp;
        private SerializedProperty edgesProp;
        private VisualElement transitionList;

        public GraphNodeInspector(GraphData graphData)
        {
            this.graphData = graphData;

            AddToClassList(SMEditorUtil.StateNodeInspectorClassName);
        }

        public void SetNode(NodeDataType data)
        {
            Clear();

            nodeData = data;
            nodeProp = GraphViewEditorUtil.GetElementProp(graphData, nodeData.ID);
            edgesProp = nodeProp.FindPropertyRelative("edges");

            var nameField = CreateNameField();
            var transitions = CreateTransitions();

            BuildInspector(nameField, transitions);
        }

        protected virtual void BuildInspector(VisualElement nameField, VisualElement transitions)
        {
            Add(nameField);
            Add(transitions);
        }

        private VisualElement CreateNameField()
        {
            var nameProp = nodeProp.FindPropertyRelative("name");

            var nameField = new TextField("Name");
            nameField.BindProperty(nameProp);

            return nameField;
        }

        private VisualElement CreateTransitions()
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
            return transitionContainer;
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
