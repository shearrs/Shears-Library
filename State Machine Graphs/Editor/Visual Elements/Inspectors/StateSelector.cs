using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class StateSelector : VisualElement
    {
        private static readonly Type EMPTY_STATE_TYPE = typeof(EmptyState);

        private readonly SerializedProperty stateTypeProp;
        private readonly SerializedProperty stateNameProp;
        private readonly Button button;

        public StateSelector(SerializedProperty stateTypeProp)
        {
            this.stateTypeProp = stateTypeProp;
            stateNameProp = stateTypeProp.FindPropertyRelative("name");

            AddToClassList(SMEditorUtil.StateSelectorClassName);

            button = CreateStateButton();

            Add(button);
        }

        private Button CreateStateButton()
        {
            var stateButton = new Button(ShowContextMenu);

            SetButtonText(stateButton);

            stateButton.TrackPropertyValue(stateTypeProp, (prop) => SetButtonText(stateButton));

            return stateButton;
        }

        private void SetButtonText(Button button)
        {
            button.text = stateNameProp.stringValue;
        }

        // CREATE SUB MENUS OUT OF HIERARCHY CHAINS (OR MAYBE EVEN ASSEMBLY DEFINITIONS)
        private void ShowContextMenu()
        {
            GenericMenu menu = new();

            menu.AddItem(new GUIContent("Empty State"), false, SetEmptyState);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type == EMPTY_STATE_TYPE || type.IsAbstract)
                        continue;

                    if (type.IsSubclassOf(typeof(State)))
                        menu.AddItem(new GUIContent(type.Name), false, () => SetState(type));
                }
            }

            menu.ShowAsContext();
        }

        private void SetEmptyState()
        {
            stateTypeProp.boxedValue = new SerializableSystemType(EMPTY_STATE_TYPE);

            stateTypeProp.serializedObject.ApplyModifiedProperties();
        }

        private void SetState(Type type)
        {
            stateTypeProp.boxedValue = new SerializableSystemType(type);

            stateTypeProp.serializedObject.ApplyModifiedProperties();
        }
    }
}
