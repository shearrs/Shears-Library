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
            transitionsContainer.Add(title);

            foreach (var transitionProp in transitionProps)
            {
                var transitionElement = new VisualElement();
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

                transitionElement.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
                transitionElement.style.borderBottomColor = new Color(0.05f, 0.05f, 0.05f);
                transitionElement.style.borderRightColor = new Color(0.05f, 0.05f, 0.05f);
                transitionElement.style.borderTopColor = new Color(0.05f, 0.05f, 0.05f);
                transitionElement.style.borderLeftColor = new Color(0.05f, 0.05f, 0.05f);
                transitionElement.style.borderTopWidth = 1;
                transitionElement.style.borderBottomWidth = 1;
                transitionElement.style.borderLeftWidth = 1;
                transitionElement.style.borderRightWidth = 1;
                transitionElement.style.borderBottomLeftRadius = 4;
                transitionElement.style.borderBottomRightRadius = 4;
                transitionElement.style.borderTopLeftRadius = 4;
                transitionElement.style.borderTopRightRadius = 4;
                transitionElement.style.marginBottom = 4;
                transitionElement.style.paddingTop = 4;
                transitionElement.style.paddingBottom = 4;
                transitionElement.style.paddingLeft = 4;
                transitionElement.style.paddingRight = 4;
                transitionElement.style.flexDirection = FlexDirection.Row;

                transitionElement.Add(fromLabel);
                transitionElement.Add(symbolLabel);
                transitionElement.Add(toLabel);
                transitionsContainer.Add(transitionElement);
            }

            Add(transitionsContainer);
        }
    }
}
