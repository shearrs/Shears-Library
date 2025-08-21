using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    //    [Header("State Machine")]
    //    [SerializeField] private StateMachineGraph graphData;
    //    [SerializeField] private List<StateInjectReference> injectReferences;
    //    [SerializeReference] private List<State> stateTree = new();

    //#if UNITY_EDITOR
    //    [Header("Parameters")]
    //    [SerializeReference] private List<Parameter> parameterDisplay = new();
    //    [SerializeField] private List<LocalParameterProvider> externalParameters = new();
    //#endif

    [CustomEditor(typeof(StateMachine))]
    public class StateMachineEditor : UnityEditor.Editor
    {
        private readonly HashSet<Type> injectTargets = new();
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

            foreach (var state in states)
            {
                var type = state.StateType.SystemType;
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var field in fields)
                {
                    if (!field.IsDefined(typeof(StateInjectAttribute), false))
                        continue;

                    var fieldType = field.FieldType;

                    if (injectTargets.Contains(fieldType))
                        continue;

                    injectTargets.Add(fieldType);
                }
            }

            for (int i = 0; i < injectReferencesProp.arraySize; i++)
            {
                var reference = injectReferencesProp.GetArrayElementAtIndex(i).boxedValue as StateInjectReference;

                if (!injectTargets.Contains(reference.Type))
                {
                    injectReferencesProp.DeleteArrayElementAtIndex(i);
                    i--;
                }
            }

            foreach (var target in injectTargets)
            {
                if (stateMachine.HasInjectedReference(target))
                    continue;

                injectReferencesProp.InsertArrayElementAtIndex(injectReferencesProp.arraySize);

                var element = injectReferencesProp.GetArrayElementAtIndex(injectReferencesProp.arraySize - 1);
                var injectReference = new StateInjectReference(target);

                element.boxedValue = injectReference;
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}
