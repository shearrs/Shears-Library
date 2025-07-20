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
        private SerializedProperty stateProp;

        public StateNodeInspector(GraphData graphData) : base(graphData)
        {
        }

        protected override void BuildInspector(VisualElement nameField, VisualElement transitions)
        {
            stateProp = nodeProp.FindPropertyRelative("state");

            Add(nameField);
            Add(CreateStateField());
            Add(transitions);
        }

        private VisualElement CreateStateField()
        {
            var stateButton = new Button(ShowContextMenu);

            SetButtonText(stateButton, stateProp);

            stateButton.TrackPropertyValue(stateProp, (prop) => SetButtonText(stateButton, prop));

            stateButton.style.marginTop = 8;
            stateButton.style.marginBottom = 8;
            stateButton.style.height = 30;

            return stateButton;
        }

        private void SetButtonText(Button button, SerializedProperty stateProp)
        {
            if (stateProp.boxedValue != null)
                button.text = stateProp.boxedValue.GetType().Name;
            else
                button.text = "Select State";
        }

        // CREATE SUB MENUS OUT OF HIERARCHY CHAINS (OR MAYBE EVEN ASSEMBLY DEFINITIONS)
        private void ShowContextMenu()
        {
            GenericMenu menu = new();

            menu.AddItem(new GUIContent("Clear State"), false, ClearState);

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

        private void ClearState()
        {
            stateProp.boxedValue = null;

            stateProp.serializedObject.ApplyModifiedProperties();
        }

        private void SetState(Type type)
        {
            stateProp.boxedValue = (State)Activator.CreateInstance(type);

            stateProp.serializedObject.ApplyModifiedProperties();
        }
    }
}
