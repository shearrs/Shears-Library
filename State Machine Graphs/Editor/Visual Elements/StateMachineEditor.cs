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
        private SerializedProperty stateTreeProp;
        private SerializedProperty parameterDisplayProp;
        private SerializedProperty externalParametersProp;

        private Foldout injectedEntryContainer;
        private Foldout runtimeContainer;

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement()
            {
                name = "State Machine Editor"
            };

            root.AddStyleSheet(ShearsStyles.InspectorStyles);

            injectedReferencesProp = serializedObject.FindProperty("injectedReferences");
            stateTreeProp = serializedObject.FindProperty("stateTree");
            parameterDisplayProp = serializedObject.FindProperty("parameterDisplay");
            externalParametersProp = serializedObject.FindProperty("externalParameters");
            var graphDataProp = serializedObject.FindProperty("graphData");

            CreateInjectionContainer();
            CreateRuntimeContainer();
            var graphDataField = CreateGraphDataField(graphDataProp);

            UpdateGraphFields(graphDataProp);

            root.TrackPropertyValue(graphDataProp, UpdateGraphFields);
            root.AddAll(graphDataField, injectedEntryContainer, runtimeContainer);

            return root;
        }

        private void CreateInjectionContainer()
        {
            injectedEntryContainer = new Foldout()
            {
                text = "Injected References",
                name = "Injected Entry Container",
                value = false
            };
            injectedEntryContainer.AddToClassList(ShearsStyles.DarkFoldoutClass);
        }

        private void CreateRuntimeContainer()
        {
            runtimeContainer = new Foldout()
            {
                text = "Runtime Info",
                name = "Runtime Container",
                value = false
            };

            runtimeContainer.AddToClassList(ShearsStyles.DarkFoldoutClass);
        }

        private VisualElement CreateGraphDataField(SerializedProperty graphDataProp)
        {
            var graphDataField = new PropertyField(graphDataProp);

            graphDataField.AddToClassList(ShearsStyles.DarkContainerClass);
            graphDataField.style.marginTop = 4;

            return graphDataField;
        }

        private void UpdateGraphFields(SerializedProperty graphDataProp)
        {
            RefreshInjectReferences(graphDataProp);
            UpdateRuntimeFields(graphDataProp);
        }

        private void UpdateRuntimeFields(SerializedProperty graphDataProp)
        {
            runtimeContainer.Clear();

            if (graphDataProp.objectReferenceValue == null)
            {
                runtimeContainer.style.display = DisplayStyle.None;
                return;
            }
            else
                runtimeContainer.style.display = DisplayStyle.Flex;

            var stateTreeField = new PropertyField(stateTreeProp);
            var parameterDisplayField = new PropertyField(parameterDisplayProp);
            var externalParametersField = new PropertyField(externalParametersProp);

            stateTreeField.Bind(serializedObject);
            parameterDisplayField.Bind(serializedObject);
            externalParametersField.Bind(serializedObject);

            runtimeContainer.AddAll(stateTreeField, parameterDisplayField, externalParametersField);
        }

        private void RefreshInjectReferences(SerializedProperty graphDataProp)
        {
            injectedEntryContainer.Clear();

            if (graphDataProp.objectReferenceValue == null)
            {
                injectedEntryContainer.style.display = DisplayStyle.None;
                return;
            }
            else
                injectedEntryContainer.style.display = DisplayStyle.Flex;

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

            for (int i = 0; i < entriesProp.arraySize; i++)
            {
                var entryProp = entriesProp.GetArrayElementAtIndex(i);
                var valueProp = entryProp.FindPropertyRelative("value");

                var valueField = new PropertyField(valueProp);
                valueField.Bind(serializedObject);

                injectedEntryContainer.Add(valueField);
            }
        }
    }
}
