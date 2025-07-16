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

            var transitionContainer = new VisualElement();
            transitionContainer.AddToClassList(SMEditorUtil.TransitionContainerClassName);

            foreach (var transition in transitionProps)
            {
                var transitionField = new PropertyField();
                transitionField.BindProperty(transition);

                transitionContainer.Add(transitionField);
            }

            Add(transitionContainer);
        }
    }
}
