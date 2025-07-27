using System;
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

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type == EMPTY_STATE_TYPE || type == EXTERNAL_STATE_TYPE || type.IsAbstract)
                        continue;

                    if (type.IsSubclassOf(STATE_TYPE))
                        menu.AddItem(new GUIContent(GetTypePath(type)), false, () => SetState(type));
                }
            }

            menu.ShowAsContext();
        }

        private string GetTypePath(Type type)
        {
            stringBuilder.Clear();

            var name = StringUtil.PascalSpace(type.Name);
            stringBuilder.Append(name);

            while (type.BaseType != null && type.BaseType != STATE_TYPE)
            {
                type = type.BaseType;
                name = StringUtil.PascalSpace(type.Name);
                
                stringBuilder.Insert(0, StringUtil.PascalSpace(name));
                stringBuilder.Insert(name.Length, '/');
            }

            return stringBuilder.ToString();
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
