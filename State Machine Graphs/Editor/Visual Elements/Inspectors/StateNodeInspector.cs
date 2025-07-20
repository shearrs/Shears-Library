using Shears.GraphViews;
using System;
using System.Data.Common;
using UnityEditor;
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
            Add(nameField);
            Add(CreateStateField());
            Add(transitions);
        }

        private VisualElement CreateStateField()
        {
            var stateButton = new Button(ShowContextMenu)
            {
                text = "State Type"
            };

            stateButton.style.marginTop = 8;
            stateButton.style.marginBottom = 8;
            stateButton.style.height = 30;

            return stateButton;
        }

        // CREATE SUB MENUS OUT OF HIERARCHY CHAINS (OR MAYBE EVEN ASSEMBLY DEFINITIONS)
        private void ShowContextMenu()
        {
            GenericMenu menu = new();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(State)) && !type.IsAbstract)
                        menu.AddItem(new GUIContent(type.Name), false, () => SetState(type));
                }
            }

            menu.ShowAsContext();
        }

        private void SetState(Type type)
        {
            Debug.Log("set state to new instance of " + type.Name);
        }
    }
}
