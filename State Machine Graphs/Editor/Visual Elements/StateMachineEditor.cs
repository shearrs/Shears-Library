using Shears.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    [CustomEditor(typeof(StateMachine))]
    public class StateMachineEditor : UnityEditor.Editor
    {
        private readonly List<StateInjectTarget> injectTargets = new();
        private readonly Dictionary<Type, ObjectField> injectFields = new();
        private VisualElement root;

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();

            var graphDataProp = serializedObject.FindProperty("graphData");
            var graphDataField = new PropertyField(graphDataProp);

            root.Add(graphDataField);

            if (graphDataProp.objectReferenceValue != null)
            {
                var graph = graphDataProp.objectReferenceValue as StateMachineGraph;

                UpdateInjectTargets(graph);
            }

            return root;
        }

        private void UpdateInjectTargets(StateMachineGraph graph)
        {
            var graphSO = new SerializedObject(graph);
            var states = graph.GetStateNodes();
            injectTargets.Clear();

            foreach (var state in states)
            {
                if (state is not IStateInjectable injectable)
                    continue;

                var injectableTypes = injectable.GetInjectableTypes();

                var stateType = state.StateType.SystemType;
                var fields = stateType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
                foreach (var field in fields)
                {
                    var fieldType = field.FieldType;

                    if (!injectableTypes.Contains(fieldType))
                        continue;

                    injectTargets.Add(new(state.ID, field.Name, fieldType));
                }
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}
