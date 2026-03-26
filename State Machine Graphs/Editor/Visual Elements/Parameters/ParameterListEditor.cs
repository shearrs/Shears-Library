using Shears.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    [CustomEditor(typeof(ParameterList))]
    public class ParameterListEditor : UnityEditor.Editor
    {
        private VisualElement parameterList;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var script = Shears.Editor.VisualElementUtil.CreateScriptField(serializedObject);
            var addButton = CreateAddButton();

            script.style.marginBottom = 4;

            var parametersProp = serializedObject.FindProperty("parameters");

            parameterList = new ScrollView(ScrollViewMode.Vertical);
            CreateParameterList();
            parameterList.TrackPropertyValue(parametersProp, _ => CreateParameterList());

            root.AddAll(script, addButton, parameterList);

            return root;
        }

        private void ShowContextMenu()
        {
            GenericMenu menu = new();

            menu.AddItem(new GUIContent("Bool Parameter"), false, AddParameter, new BoolParameterData());
            menu.AddItem(new GUIContent("Int Parameter"), false, AddParameter, new IntParameterData());
            menu.AddItem(new GUIContent("Float Parameter"), false, AddParameter, new FloatParameterData());
            menu.AddItem(new GUIContent("Trigger Parameter"), false, AddParameter, new TriggerParameterData());

            menu.ShowAsContext();
        }

        private VisualElement CreateAddButton()
        {
            var addButton = new Button(ShowContextMenu)
            {
                text = "+"
            };

            return addButton;
        }

        private void CreateParameterList()
        {
            parameterList.Clear();
            parameterList.AddStyleSheet(ShearsStyles.InspectorStyles);
            var parametersProp = serializedObject.FindProperty("parameters");

            for (int i = 0; i < parametersProp.arraySize; i++)
            {
                var parameterProp = parametersProp.GetArrayElementAtIndex(i);

                var parameterUI = new VisualElement();
                parameterUI.style.marginTop = 2;
                parameterUI.AddToClassList(ShearsStyles.DarkContainerClass);

                var nameProp = parameterProp.FindPropertyRelative("name");
                var valueProp = parameterProp.FindPropertyRelative("value");

                var nameContainer = new VisualElement();
                nameContainer.style.flexDirection = FlexDirection.Row;

                var deleteButton = new Button(() => DeleteParameter(parameterProp))
                {
                    text = "X"
                };
                deleteButton.style.position = Position.Absolute;
                deleteButton.style.width = 24;
                deleteButton.style.right = 0;

                var nameField = new PropertyField(nameProp);
                nameContainer.AddAll(nameField, deleteButton);

                var valueField = new PropertyField(valueProp);

                nameField.BindProperty(nameProp);
                valueField.BindProperty(valueProp);

                parameterUI.AddAll(nameContainer, valueField);

                parameterList.Add(parameterUI);
            }
        }

        private void AddParameter(object parameterObj)
        {
            var parametersProp = serializedObject.FindProperty("parameters");
            int arraySize = parametersProp.arraySize;

            parametersProp.InsertArrayElementAtIndex(arraySize);
            parametersProp.GetArrayElementAtIndex(arraySize).boxedValue = parameterObj;

            serializedObject.ApplyModifiedProperties();
        }

        private void DeleteParameter(SerializedProperty parameter)
        {
            var parametersProp = serializedObject.FindProperty("parameters");

            for (int i = 0; i < parametersProp.arraySize; i++)
            {
                var currentProp = parametersProp.GetArrayElementAtIndex(i);

                if (SerializedProperty.DataEquals(currentProp, parameter))
                {
                    parametersProp.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    break;
                }
            }
        }
    }
}
