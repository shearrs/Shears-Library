using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    [CustomEditor(typeof(StateMachine))]
    public class StateMachineEditor : UnityEditor.Editor
    {
        private VisualElement root;
        private SerializedProperty injectedReferencesProp;

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();

            var graphDataProp = serializedObject.FindProperty("graphData");
            var graphDataField = new PropertyField(graphDataProp);

            root.Add(graphDataField);

            if (graphDataProp.objectReferenceValue != null)
            {
                var graph = graphDataProp.objectReferenceValue as StateMachineGraph;
                injectedReferencesProp = serializedObject.FindProperty("injectedReferences");

                UpdateInjectTargets(graph);

                var injectedReferencesField = new PropertyField(injectedReferencesProp);
                root.Add(injectedReferencesField);
            }

            var stateTreeProp = serializedObject.FindProperty("stateTree");
            var stateTreeField = new PropertyField(stateTreeProp);
            root.Add(stateTreeField);

            return root;
        }

        private void UpdateInjectTargets(StateMachineGraph graph)
        {
            var stateNodes = graph.GetStateNodes();
            var injectedReferences = injectedReferencesProp.boxedValue as StateInjectReferenceDictionary;

            foreach (var stateNode in stateNodes)
            {
                if (!typeof(IStateInjectable).IsAssignableFrom(stateNode.StateType))
                    continue;

                var stateInstance = Activator.CreateInstance(stateNode.StateType) as IStateInjectable;
                var injectableTypes = stateInstance.GetInjectableTypes();

                foreach (var type in injectableTypes)
                {
                    if (injectedReferences.TryGetValue(type, out var reference))
                    {
                        reference.AddTarget(stateNode.ID);
                        continue;
                    }

                    reference = new StateInjectReference(new SerializableSystemType(type));
                    reference.AddTarget(stateNode.ID);

                    injectedReferences[type] = reference;
                }
            }

            injectedReferencesProp.boxedValue = injectedReferences;

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}
