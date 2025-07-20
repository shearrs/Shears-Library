using Shears.GraphViews;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class StateNodeInspector : GraphNodeInspector<StateNodeData>
    {
        public StateNodeInspector(GraphData graphData) : base(graphData)
        {
        }

        protected override void BuildInspector(VisualElement nameField, VisualElement transitions)
        {
            var stateProp = nodeProp.FindPropertyRelative("stateType");

            Add(nameField);
            Add(new StateSelector(stateProp));
            Add(transitions);
        }
    }
}
