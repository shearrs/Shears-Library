using Shears.GraphViews.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class StateNodeInspector : VisualElement
    {
        private readonly StateMachineGraph graphData;
        private StateNodeData nodeData;
        private SerializedProperty nodeProp;

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

            CreateNameField();
            CreateTransitions();
            // get list of edges, get actual transition serialized property from graph, bind to stuff
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
            var edgesProp = nodeProp.FindPropertyRelative("edges");
            var transitionProps = new List<SerializedProperty>();
           
            for (int i = 0; i < edgesProp.arraySize; i++)
            {
                var edge = edgesProp.GetArrayElementAtIndex(i);
                var transition = GraphViewEditorUtil.GetElementProp(graphData, edge.stringValue);
                
                if (transition != null)
                    transitionProps.Add(GraphViewEditorUtil.GetElementProp(graphData, edge.stringValue));
            }

            var transitionsContainer = new VisualElement();
            var title = new Label("Transitions");

            transitionsContainer.AddToClassList(SMEditorUtil.TransitionContainerClassName);
            title.AddToClassList(SMEditorUtil.TransitionsTitleClassName);

            transitionsContainer.Add(title);

            foreach (var transitionProp in transitionProps)
            {
                var transitionElement = new VisualElement();
                transitionElement.AddToClassList(SMEditorUtil.TransitionClassName);
                var fromLabel = new Label();
                var symbolLabel = new Label(" -> ");
                var toLabel = new Label();
                var fromIDProp = transitionProp.FindPropertyRelative("fromID");
                var toIDProp = transitionProp.FindPropertyRelative("toID");
                var fromProp = GraphViewEditorUtil.GetElementProp(graphData, fromIDProp.stringValue);
                var toProp = GraphViewEditorUtil.GetElementProp(graphData, toIDProp.stringValue);
                var fromNameProp = fromProp.FindPropertyRelative("name");
                var toNameProp = toProp.FindPropertyRelative("name");

                fromLabel.BindProperty(fromNameProp);
                toLabel.BindProperty(toNameProp);

                transitionElement.Add(fromLabel);
                transitionElement.Add(symbolLabel);
                transitionElement.Add(toLabel);
                transitionsContainer.Add(transitionElement);
            }

            Add(transitionsContainer);
        }
    }
}
