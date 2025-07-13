using Shears.GraphViews;
using Shears.GraphViews.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    [CustomEditor(typeof(StateMachineGraph))]
    public class SMGraphEditor : UnityEditor.Editor
    {
        private StateMachineGraph graphData;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            if (target is StateMachineGraph graph)
                graphData = graph;
            else
                return root;

            root.Add(new SMGraphInspector(graphData));

            return root;
        }
    }
}
