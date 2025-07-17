using Shears.GraphViews.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class TransitionEdgeInspector : VisualElement
    {
        private readonly StateMachineGraph graphData;
        private TransitionEdgeData transitionData;
        private SerializedProperty transitionProp;

        public TransitionEdgeInspector(StateMachineGraph graphData)
        {
            this.graphData = graphData;
        }
        
        public void SetTransition(TransitionEdgeData data)
        {
            Clear();

            transitionData = data;
            transitionProp = GraphViewEditorUtil.GetElementProp(graphData, transitionData.ID);

            var transitionField = new PropertyField();
            transitionField.BindProperty(transitionProp);

            Add(transitionField);
        }
    }
}
