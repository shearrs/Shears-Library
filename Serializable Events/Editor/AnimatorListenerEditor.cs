using System;
using System.Collections.Generic;
using System.Linq;
using Shears.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Events.Editor
{
    [CustomEditor(typeof(AnimatorListener))]
    public class AnimatorListenerEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement { name = nameof(AnimatorListener).PascalSpace() };
            var valuesContainer = new VisualElement { name = "Values Container" };

            var animatorProp = serializedObject.FindProperty("animator");
            var animatorField = new PropertyField(animatorProp);

            root.AddAll(animatorField, valuesContainer);

            void buildInspector(SerializedProperty animatorProp)
            {
                valuesContainer.Clear();

                if (animatorProp.objectReferenceValue == null)
                    return;

                var boolProp = serializedObject.FindProperty("boolValue");
                var floatProp = serializedObject.FindProperty("floatValue");
                var intProp = serializedObject.FindProperty("integerValue");

                var nameField = CreateNameField(serializedObject, animatorProp);
                var boolField = new PropertyField(boolProp);
                var floatField = new PropertyField(floatProp);
                var intField = new PropertyField(intProp);

                boolField.Bind(serializedObject);
                floatField.Bind(serializedObject);
                intField.Bind(serializedObject);

                valuesContainer.AddAll(nameField, boolField, floatField, intField);
            }

            animatorField.RegisterValueChangeCallback(evt => buildInspector(evt.changedProperty));
            buildInspector(animatorProp);

            return root;
        }

        private VisualElement CreateNameField(
            SerializedObject serializedObject,
            SerializedProperty animatorProp
        )
        {
            const string NONE = "None";

            var nameProp = serializedObject.FindProperty("parameterName");
            var typeProp = serializedObject.FindProperty("parameterType");

            var animator = animatorProp.objectReferenceValue as Animator;
            var parameterMap = new Dictionary<string, AnimatorControllerParameter>()
            {
                { NONE, null },
            };

            foreach (var parameter in animator.parameters)
                parameterMap[parameter.name] = parameter;

            var nameList = parameterMap.Keys.ToList();
            int startIndex;

            if (string.IsNullOrEmpty(nameProp.stringValue))
            {
                nameProp.stringValue = NONE;
                nameProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            if (nameProp.stringValue == NONE)
                startIndex = 0;
            else
                startIndex = nameList.IndexOf(nameProp.stringValue);

            if (startIndex == -1)
            {
                nameProp.stringValue = NONE;
                nameProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                startIndex = 0;
            }

            var dropdown = new DropdownField("Parameter", nameList, startIndex);
            dropdown.AddBaseFieldAlignClass();
            dropdown.RegisterValueChangedCallback(evt =>
            {
                var parameter = parameterMap[evt.newValue];

                int enumIndex =
                    parameter == null
                        ? 0
                        : Array.IndexOf(
                            Enum.GetValues(typeof(AnimatorControllerParameterType)),
                            parameter.type
                        );

                nameProp.stringValue = evt.newValue;
                typeProp.enumValueIndex = enumIndex;

                serializedObject.ApplyModifiedProperties();
            });

            return dropdown;
        }
    }
}
