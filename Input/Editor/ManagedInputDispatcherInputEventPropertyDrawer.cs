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
        private VisualElement root;
        private SerializedProperty property;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            root = new();

            this.property = property;

            var inputActions = GetInputActions();

            if (inputActions.Count == 0)
                return root;

            var nameField = CreateNameDropdown(inputActions);
            var phaseField = CreatePhaseDropdown();
            var eventField = new PropertyField(property.FindPropertyRelative("onInput"));

            root.Add(nameField);
            root.Add(phaseField);
            root.Add(eventField);

            return root;
        }

        private DropdownField CreateNameDropdown(List<string> inputActions)
        {
            var dropdown = new DropdownField("Input Name", inputActions, 0);
            string currentAction = property.FindPropertyRelative("inputName").stringValue;

            dropdown.index = GetInputActionIndex(inputActions, currentAction);

            dropdown.RegisterValueChangedCallback(OnNameValueChanged);
            SetInputAction(currentAction);

            return dropdown;
        }

        private List<string> GetInputActions()
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

            return 0;
        }

        private void OnNameValueChanged(ChangeEvent<string> evt) => SetInputAction(evt.newValue);
        private void SetInputAction(string newName)
        {
            property.FindPropertyRelative("inputName").stringValue = newName;
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
        }

        private DropdownField CreatePhaseDropdown()
        {
            List<string> phases = GetPhases();
            var dropdown = new DropdownField("Phase", phases, 0);
            ManagedInputPhase currentPhase = (ManagedInputPhase)property.FindPropertyRelative("phase").enumValueIndex;

            dropdown.index = phases.IndexOf(currentPhase.ToString());

            dropdown.RegisterValueChangedCallback(OnPhaseValueChanged);
            SetPhase(currentPhase);

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

        private void OnPhaseValueChanged(ChangeEvent<string> evt) => SetPhase(GetPhaseForName(evt.newValue));
        private void SetPhase(ManagedInputPhase phase)
        {
            property.FindPropertyRelative("phase").enumValueIndex = (int)phase;
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
        }
    }
}