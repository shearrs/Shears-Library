using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    [CustomEditor(typeof(StateMachine))]
    public class StateMachineEditor : UnityEditor.Editor
    {
        private readonly List<StateInjectTarget> injectTargets = new();
        private readonly List<StateInjectReference> injectReferences = new();
        private SerializedProperty injectReferencesProp;
        private StateMachine stateMachine;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            stateMachine = serializedObject.targetObject as StateMachine;

            var graphDataProp = serializedObject.FindProperty("graphData");
            var graphDataField = new PropertyField(graphDataProp);

            root.Add(graphDataField);

            if (graphDataProp.objectReferenceValue != null)
            {
                var graph = graphDataProp.objectReferenceValue as StateMachineGraph;
                injectReferencesProp = serializedObject.FindProperty("injectReferences");

                UpdateInjectTargets(graph);

                var injectReferencesField = new PropertyField(injectReferencesProp);
                root.Add(injectReferencesField);
            }

            return root;
        }

        private void UpdateInjectTargets(StateMachineGraph graph)
        {
            var states = graph.GetStateNodes();
            injectTargets.Clear();
            injectReferences.Clear();

            foreach (var state in states)
            {
                var type = state.StateType.SystemType;
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var field in fields)
                {
                    if (!field.IsDefined(typeof(StateInjectAttribute), false))
                        continue;

                    var fieldType = field.FieldType;

                    injectTargets.Add(new(state.ID, state.GetType(), fieldType));
                }
            }

            for (int i = 0; i < injectReferencesProp.arraySize; i++)
            {
                var reference = injectReferencesProp.GetArrayElementAtIndex(i).boxedValue as StateInjectReference;

                if (!injectTargets.Exists((t) => t.FieldType == reference.FieldType))
                {
                    injectReferencesProp.DeleteArrayElementAtIndex(i);
                    i--;
                }
            }

            // for each target, if we have the type injected, just assign the target to that reference
            // else, create a new reference and add the target to it

            // TODO: add a simple API to add a state target to the state machine and the state machine handles it
            foreach (var target in injectTargets)
            {
                if (stateMachine.HasInjectType(target.FieldType))
                {
                    for (int i = 0; i < injectReferencesProp.arraySize; i++)
                    {
                        var reference = injectReferencesProp.GetArrayElementAtIndex(i).boxedValue as StateInjectReference;

                        if (reference.FieldType == target.FieldType)
                        {
                            if (!stateMachine.HasInjectTarget(target))
                                reference.AddTarget(target);

                            break;
                        }
                    }
                }

                //if (stateMachine.HasInjectTarget(target))
                //    continue;

                //injectReferencesProp.InsertArrayElementAtIndex(injectReferencesProp.arraySize);

                //var element = injectReferencesProp.GetArrayElementAtIndex(injectReferencesProp.arraySize - 1);
                //var injectReference = new StateInjectReference(target);

                //element.boxedValue = injectReference;
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}
