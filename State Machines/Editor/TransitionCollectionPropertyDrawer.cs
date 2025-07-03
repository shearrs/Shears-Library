using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachines.Editor
{
    [CustomPropertyDrawer(typeof(TransitionCollection))]
    public class TransitionCollectionPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.styleSheets.Add(Resources.Load<StyleSheet>("StateMachines"));

            var transitionsProp = property.FindPropertyRelative("transitions");

            var transitionsContainer = CreateTransitionsContainer(property, transitionsProp);

            root.Add(transitionsContainer);

            return root;
        }

        private Foldout CreateTransitionsContainer(SerializedProperty property, SerializedProperty transitionsProp)
        {
            var transitionsContainer = new Foldout
            {
                text = "Transitions",
                value = property.isExpanded
            };

            transitionsContainer.RegisterValueChangedCallback(evt =>
            {
                property.isExpanded = evt.newValue;
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            });

            UpdateTransitionsContainer(transitionsContainer, transitionsProp);

            return transitionsContainer;
        }

        private void UpdateTransitionsContainer(VisualElement transitionsContainer, SerializedProperty transitionsProp)
        {
            transitionsContainer.Clear();

            var addTransitionButton = CreateAddTransitionButton(transitionsProp, () => UpdateTransitionsContainer(transitionsContainer, transitionsProp));
            transitionsContainer.Add(addTransitionButton);

            for (int i = 0; i < transitionsProp.arraySize; i++)
            {
                var transitionField = CreateTransitionField(transitionsProp, i, () => UpdateTransitionsContainer(transitionsContainer, transitionsProp));

                transitionsContainer.Add(transitionField);
            }
        }

        private VisualElement CreateTransitionField(SerializedProperty transitionsProp, int index, Action updateCallback)
        {
            var transitionProp = transitionsProp.GetArrayElementAtIndex(index);

            var transitionFoldout = new Foldout
            {
                name = $"Transition Wrapper {index}",
                value = transitionProp.FindPropertyRelative("isFoldoutExpanded").boolValue
            };

            transitionFoldout.RegisterValueChangedCallback(evt =>
            {
                transitionProp.FindPropertyRelative("isFoldoutExpanded").boolValue = evt.newValue;
                transitionProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            });

            void updateFoldoutText(SerializedProperty toProp)
            {
                var state = (State)toProp.objectReferenceValue;

                string newText = state != null ? state.Name : "None";

                transitionFoldout.text = newText;
            }

            var toProp = transitionProp.FindPropertyRelative("to");
            transitionFoldout.TrackPropertyValue(toProp, updateFoldoutText);
            updateFoldoutText(toProp);

            var transitionField = new PropertyField()
            {
                name = $"Transition {index}"
            };
            transitionField.BindProperty(transitionProp);

            transitionField.AddToClassList("transition-field");

            var deleteTransitionButton = new Button(() => RemoveTransition(transitionsProp, index, updateCallback))
            {
                name = $"Delete Transition {index}",
                text = "Remove",
            };

            deleteTransitionButton.AddToClassList("delete-transition-button");

            transitionFoldout.Add(transitionField);
            transitionFoldout.Add(deleteTransitionButton);

            return transitionFoldout;
        }

        private Button CreateAddTransitionButton(SerializedProperty transitionsProp, Action updateCallback)
        {
            var addTransitionButton = new Button(() => AddTransition(transitionsProp, updateCallback))
            {
                text = "Add Transition"
            };

            return addTransitionButton;
        }

        private void AddTransition(SerializedProperty transitionsProp, Action updateCallback)
        {
            transitionsProp.InsertArrayElementAtIndex(transitionsProp.arraySize);
            SerializedProperty newTransitionProp = transitionsProp.GetArrayElementAtIndex(transitionsProp.arraySize - 1);

            State fromState = (State)transitionsProp.serializedObject.targetObject;
            newTransitionProp.boxedValue = new Transition(fromState, null);

            newTransitionProp.serializedObject.ApplyModifiedProperties();

            updateCallback();
        }

        private void RemoveTransition(SerializedProperty transitionsProp, int index, Action updateCallback)
        {
            transitionsProp.DeleteArrayElementAtIndex(index);
            transitionsProp.serializedObject.ApplyModifiedProperties();

            updateCallback();
        }
    }
}
