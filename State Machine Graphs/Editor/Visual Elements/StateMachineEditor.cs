using Shears.Editor;
using System;
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
        private VisualElement injectedEntryContainer;

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement()
            {
                name = "State Machine Editor"
            };
            injectedEntryContainer = new()
            {
                name = "Injected Entry Container"
            };
            injectedEntryContainer.SetAllPadding(4);
            injectedReferencesProp = serializedObject.FindProperty("injectedReferences");

            var graphDataProp = serializedObject.FindProperty("graphData");
            var graphDataField = new PropertyField(graphDataProp);

            var stateTreeProp = serializedObject.FindProperty("stateTree");
            var stateTreeField = new PropertyField(stateTreeProp);

            var parameterDisplayProp = serializedObject.FindProperty("parameterDisplay");
            var parameterDisplayField = new PropertyField(parameterDisplayProp);

            var externalParametersProp = serializedObject.FindProperty("externalParameters");
            var externalParametersField = new PropertyField(externalParametersProp);

            root.AddAll(graphDataField, injectedEntryContainer);

            //graphDataField.RegisterValueChangeCallback((evt) => RefreshInjectReferences(evt.changedProperty));
            RefreshInjectReferences(graphDataProp);

            return root;
        }

        private void RefreshInjectReferences(SerializedProperty graphDataProp)
        {
            Debug.Log("refresh");

            if (graphDataProp.objectReferenceValue == null)
            {
                injectedEntryContainer.Clear();
                return;
            }

            Debug.Log("inject");
            var graph = graphDataProp.objectReferenceValue as StateMachineGraph;

            UpdateInjectReferences(graph);
            CreateInjectReferenceFields();
        }

        private void UpdateInjectReferences(StateMachineGraph graph)
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
    
        private void CreateInjectReferenceFields()
        {
            injectedEntryContainer.Clear();

            var entriesProp = injectedReferencesProp.FindPropertyRelative("entries");
            var header = VisualElementUtil.CreateHeader("Injected References");

            injectedEntryContainer.Add(header);

            for (int i = 0; i < entriesProp.arraySize; i++)
            {
                var entryProp = entriesProp.GetArrayElementAtIndex(i);
                var valueProp = entryProp.FindPropertyRelative("value");

                var valueField = new PropertyField(valueProp);
                injectedEntryContainer.Add(valueField);

                Debug.Log("add value: " + valueProp.boxedValue);
            }
        }
    }
}
