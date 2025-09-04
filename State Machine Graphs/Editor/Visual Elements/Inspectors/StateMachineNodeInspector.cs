using Shears.GraphViews;
using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class StateMachineNodeInspector : GraphNodeInspector<StateMachineNodeData>
    {
        private readonly VisualElement referenceHintContainer;

        public StateMachineNodeInspector(GraphData graphData) : base(graphData)
        {
            referenceHintContainer = new VisualElement()
            {
                name = "Reference Hint Container"
            };
        }

        protected override void BuildInspector(VisualElement nameField, VisualElement transitions)
        {
            var stateTypeProp = nodeProp.FindPropertyRelative("stateType");

            Add(nameField);
            Add(new StateSelector(stateTypeProp));
            Add(referenceHintContainer);
            Add(transitions);

            AddReferenceHints();
            referenceHintContainer.TrackPropertyValue(stateTypeProp, (prop) => AddReferenceHints());
        }

        private void AddReferenceHints()
        {
            referenceHintContainer.Clear();

            if (nodeData.StateType == SerializableSystemType.Empty)
                return;

            if (!typeof(IStateInjectable).IsAssignableFrom(nodeData.StateType))
                return;

            var stateInstance = Activator.CreateInstance(nodeData.StateType) as IStateInjectable;
            var injectableTypes = stateInstance.GetInjectableTypes();

            foreach (var type in injectableTypes)
            {
                var label = new Label($"Requires: {StringUtil.PascalSpace(type.Name)}");

                referenceHintContainer.Add(label);
            }
        }
    }
}
