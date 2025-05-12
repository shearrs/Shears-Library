using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Shears.Input.Editor
{
    [CustomPropertyDrawer(typeof(ManagedInputDispatcher.InputEvent))]
    public class ManagedInputDispatcherInputEventPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var inputActions = GetInputActions(property);

            if (inputActions.Count == 0)
                return root;

            var nameField = CreateNameDropdown(inputActions, property);
            var phaseField = CreatePhaseDropdown(property);
            var eventField = new PropertyField(property.FindPropertyRelative("onInput"));

            var foldout = CreateFoldout(nameField.value, property);

            nameField.RegisterValueChangedCallback(evt =>
            {
                foldout.text = evt.newValue;
            });

            foldout.Add(nameField);
            foldout.Add(phaseField);
            foldout.Add(eventField);

            root.Add(foldout);

            return root;
        }

        #region Input Action Field
        private DropdownField CreateNameDropdown(List<string> inputActions, SerializedProperty property)
        {
            var dropdown = new DropdownField("Input Name", inputActions, 0);
            string currentAction = property.FindPropertyRelative("inputName").stringValue;

            int index = GetInputActionIndex(inputActions, currentAction);

            if (index == -1)
            {
                currentAction = "MISSING ACTION";
                dropdown.value = "MISSING ACTION";
            }
            else
            {
                dropdown.index = index;
                currentAction = dropdown.value;
            }

            void setInputAction(string newName)
            {
                property.FindPropertyRelative("inputName").stringValue = newName;
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }

            dropdown.RegisterValueChangedCallback(evt => setInputAction(evt.newValue));
            setInputAction(currentAction);

            return dropdown;
        }

        private List<string> GetInputActions(SerializedProperty property)
        {
            List<string> inputActions = new();

            var inputProviderProp = property.serializedObject.FindProperty("inputProvider");

            if (inputProviderProp.objectReferenceValue == null || inputProviderProp.objectReferenceValue is not ManagedInputMap inputMap)
                return inputActions;

            SerializedObject inputMapSO = new(inputMap);

            var inputActionAsset = inputMapSO.FindProperty("inputActions").objectReferenceValue as InputActionAsset;
            string actionMapName = inputMapSO.FindProperty("actionMapName").stringValue;

            var actionMap = inputActionAsset.FindActionMap(actionMapName, true);

            foreach (var action in actionMap)
                inputActions.Add(action.name);

            return inputActions;
        }

        private int GetInputActionIndex(List<string> inputActions, string name)
        {
            for (int i = 0; i < inputActions.Count; i++)
            {
                if (inputActions[i] == name)
                    return i;
            }

            if (name == string.Empty)
                return 0;
            else
                return -1;
        }
        #endregion

        #region Phase Field
        private DropdownField CreatePhaseDropdown(SerializedProperty property)
        {
            List<string> phases = GetPhases();
            var dropdown = new DropdownField("Phase", phases, 0);
            ManagedInputPhase currentPhase = (ManagedInputPhase)property.FindPropertyRelative("phase").enumValueIndex;

            dropdown.index = phases.IndexOf(currentPhase.ToString());

            void setPhase(ManagedInputPhase phase)
            {
                property.FindPropertyRelative("phase").enumValueIndex = (int)phase;
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }

            dropdown.RegisterValueChangedCallback(evt => setPhase(GetPhaseForName(evt.newValue)));
            setPhase(currentPhase);

            return dropdown;
        }

        private List<string> GetPhases()
        {
            List<string> phases = new();

            foreach (var phase in Enum.GetValues(typeof(ManagedInputPhase)))
            {
                phases.Add(phase.ToString());
            }

            return phases;
        }

        private ManagedInputPhase GetPhaseForName(string name)
        {
            foreach (var phase in Enum.GetValues(typeof(ManagedInputPhase)))
            {
                if (name == phase.ToString())
                    return (ManagedInputPhase)phase;
            }

            return default;
        }
        #endregion

        #region Foldout
        private Foldout CreateFoldout(string name, SerializedProperty property)
        {
            var isExpandedProp = property.FindPropertyRelative("isExpanded");

            var foldout = new Foldout
            {
                text = name,
                value = property.FindPropertyRelative("isExpanded").boolValue
            };

            foldout.RegisterValueChangedCallback(evt =>
            {
                isExpandedProp.boolValue = foldout.value;

                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            });

            return foldout;
        }
        #endregion
    }
}