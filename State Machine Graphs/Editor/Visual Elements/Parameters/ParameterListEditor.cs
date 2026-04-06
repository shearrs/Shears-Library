using Shears.Editor;
using System.Collections.Generic;
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
            var script = Shears.Editor.VisualElementEditorUtil.CreateScriptField(serializedObject);
            var addButton = CreateAddButton();

            script.style.marginBottom = 4;

            var parameterList = CreateParameterList();

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

        private VisualElement CreateParameterList()
        {
            var parametersProp = serializedObject.FindProperty("parameters");

            VisualElement createElement()
            {
                var parameterUI = new VisualElement();
                var nameField = new PropertyField();
                var valueField = new PropertyField();

                parameterUI.style.paddingTop = 4;
                parameterUI.style.paddingBottom = 4;

                parameterUI.AddAll(nameField, valueField);

                return parameterUI;
            }

            void bindElement(VisualElement element, int index)
            {
                var nameField = element.hierarchy[0] as PropertyField;
                var valueField = element.hierarchy[1] as PropertyField;

                nameField.BindProperty(parametersProp.GetArrayElementAtIndex(index).FindPropertyRelative("name"));
                valueField.BindProperty(parametersProp.GetArrayElementAtIndex(index).FindPropertyRelative("value"));
            }

            var view = new ListView()
            {
                showAddRemoveFooter = true,
                allowAdd = false,
                showBorder = true,
                reorderable = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                fixedItemHeight = 40,
                showBoundCollectionSize = false,
            };

            view.BindProperty(parametersProp);
            view.makeItem = createElement;
            view.bindItem = bindElement;

            return view;
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
