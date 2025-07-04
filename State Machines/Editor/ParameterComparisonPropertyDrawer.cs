using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachines.Editor
{
    [CustomPropertyDrawer(typeof(ParameterComparison), true)]
    public class ParameterComparisonPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.styleSheets.Add(Resources.Load<StyleSheet>("StateMachines"));

            var parameterField = CreateParameterField(property);
            var compareValueField = CreateCompareValueField(property);

            root.Add(parameterField);
            root.Add(compareValueField);

            return root;
        }

        #region Parameter Field
        protected virtual VisualElement CreateParameterField(SerializedProperty property)
        {
            var parameterField = new DropdownField("Parameter");

            var stateMachineParameters = GetStateMachineParameterNames(property);

            // Add all of the parameters in the state machine to the dropdown field options
            foreach (var parameterName in stateMachineParameters)
                parameterField.choices.Add(parameterName);

            var parameterIDProp = property.FindPropertyRelative("parameterID");
            string name = "";

            // If our comparison already has a parameter reference, we can set the name of the parameter in the dropdown field
            // to the name of the parameter in the state machine
            if (parameterIDProp != null && parameterIDProp.stringValue != string.Empty)
            {
                var parameter = GetStateMachineParameterByID(property, parameterIDProp.stringValue);

                // If the previous parameter reference is no longer in the state machine, we set the parameter to null
                if (parameter == null)
                {
                    parameterIDProp.stringValue = string.Empty;
                    parameterIDProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
                else // else, we set the name of the parameter in the dropdown field to the name of the parameter in the state machine
                    name = parameter.Name;
            }

            parameterField.value = name;

            // When a value is selected in the dropdown field, we set the ParameterComparison parameter to the parameter with the selected name
            parameterField.RegisterValueChangedCallback(evt =>
            {
                var selectedParameterName = evt.newValue;
                var selectedParameter = GetStateMachineParameterByName(property, selectedParameterName);

                if (selectedParameter != null)
                    parameterIDProp.stringValue = selectedParameter.ID;
                else
                {
                    Debug.LogWarning($"Selected parameter '{selectedParameterName}' is null!");
                    parameterIDProp.stringValue = string.Empty;
                }

                property.serializedObject.ApplyModifiedProperties();
            });

            return parameterField;
        }

        private IReadOnlyList<string> GetStateMachineParameterNames(SerializedProperty property)
        {
            var parameterNames = new List<string>();

            var stateMachine = GetStateMachine(property);
            if (stateMachine == null)
            {
                Debug.LogError("StateMachine is null! Cannot retrieve parameters.");
                return parameterNames;
            }

            var stateMachineSO = new SerializedObject(stateMachine);
            var parametersProp = stateMachineSO.FindProperty("parameters");

            if (parametersProp == null || parametersProp.isArray == false)
            {
                Debug.LogError("StateMachine parameters property is null or not an array!");
                return parameterNames;
            }

            var comparisonType = GetBaseGenericArgument(property.boxedValue.GetType());

            for (int i = 0; i < parametersProp.arraySize; i++)
            {
                var parameterProperty = parametersProp.GetArrayElementAtIndex(i);
                var parameter = parameterProperty.boxedValue as Parameter;

                if (GetBaseGenericArgument(parameter.GetType()) == comparisonType)
                    parameterNames.Add(parameter.Name);
            }

            return parameterNames;
        }

        private Parameter GetStateMachineParameterByID(SerializedProperty property, string id)
        {
            var parametersProp = GetStateMachineParameters(property);

            for (int i = 0; i < parametersProp.arraySize; i++)
            {
                var parameterProp = parametersProp.GetArrayElementAtIndex(i);
                var parameter = parameterProp.boxedValue as Parameter;

                if (parameter.ID == id)
                    return parameter;
            }

            return null;
        }

        private Parameter GetStateMachineParameterByName(SerializedProperty property, string parameterName)
        {
            var parametersProp = GetStateMachineParameters(property);

            for (int i = 0; i < parametersProp.arraySize; i++)
            {
                var parameterProp = parametersProp.GetArrayElementAtIndex(i);
                var parameter = parameterProp.boxedValue as Parameter;

                if (parameter.Name == parameterName)
                    return parameter;
            }

            return null;
        }

        private SerializedProperty GetStateMachineParameters(SerializedProperty property)
        {
            var stateMachine = GetStateMachine(property);

            if (stateMachine == null)
                return null;

            var stateMachineSO = new SerializedObject(stateMachine);
            var parametersProp = stateMachineSO.FindProperty("parameters");

            return parametersProp;
        }

        private StateMachineBase GetStateMachine(SerializedProperty property)
        {
            var state = (State)property.serializedObject.targetObject;

            if (state == null)
            {
                Debug.LogError("State is null!");
                return null;
            }

            Transform currentTransform = state.transform;
            while (currentTransform != null)
            {
                if (currentTransform.TryGetComponent<StateMachineBase>(out var stateMachine))
                    return stateMachine;

                currentTransform = currentTransform.parent;
            }

            return null;
        }

        private Type GetBaseGenericArgument(Type type)
        {
            var genericType = type;

            while (!genericType.IsGenericType && genericType.BaseType != null)
                genericType = genericType.BaseType;

            if (!genericType.IsGenericType)
            {
                Debug.LogError("Generic comparison type not found!");
                return null;
            }

            var comparisonType = genericType.GetGenericArguments()[0];

            return comparisonType;
        }
        #endregion
    
        #region Compare Value Field
        protected virtual VisualElement CreateCompareValueField(SerializedProperty property)
        {
            var compareValueField = new PropertyField(property.FindPropertyRelative("compareValue"));
            
            return compareValueField;
        }
        #endregion
    }
}
