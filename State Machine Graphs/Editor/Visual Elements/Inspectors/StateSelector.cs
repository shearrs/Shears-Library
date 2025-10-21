using System;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class StateSelector : VisualElement
    {
        private static readonly Type STATE_TYPE = typeof(State);
        private static readonly Type EMPTY_STATE_TYPE = typeof(EmptyState);
        private static readonly Type EXTERNAL_STATE_TYPE = typeof(ExternalGraphState);

        private readonly SerializedProperty stateTypeProp;
        private readonly SerializedProperty stateNameProp;
        private readonly Button button;
        private readonly StringBuilder stringBuilder;

        public StateSelector(SerializedProperty stateTypeProp)
        {
            this.stateTypeProp = stateTypeProp;
            stateNameProp = stateTypeProp.FindPropertyRelative("prettyName");
            stringBuilder = new(128);

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
            var types = TypeCache.GetTypesWithAttribute<StateMenuItem>();

            foreach (var type in types)
            {
                if (type.IsAbstract)
                    continue;

                if (type.IsSubclassOf(STATE_TYPE))
                    TryAddMenuItem(menu, type);
            }

            menu.ShowAsContext();
        }

        private void TryAddMenuItem(GenericMenu menu, Type type)
        {
            var attribute = type.GetCustomAttribute<StateMenuItem>();

            if (attribute == null)
                return;

            menu.AddItem(new GUIContent(attribute.MenuPath), false, () => SetState(type));
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
