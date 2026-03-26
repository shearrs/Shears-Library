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
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var script = Shears.Editor.VisualElementUtil.CreateScriptField(serializedObject);
            var addButton = CreateAddButton();

            script.style.marginBottom = 4;

            var parametersProp = serializedObject.FindProperty("parameters");
            var parametersField = new PropertyField(parametersProp);

            root.AddAll(script, addButton, parametersField);

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

        private void AddParameter(object parameterObj)
        {
            var parametersProp = serializedObject.FindProperty("parameters");
            int arraySize = parametersProp.arraySize;

            parametersProp.InsertArrayElementAtIndex(arraySize);
            parametersProp.GetArrayElementAtIndex(arraySize).boxedValue = parameterObj;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
