using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    //[Header("Parameters")]
    //[SerializeReference] private List<Parameter> parameterDisplay = new();
    //[SerializeField] private List<LocalParameterProvider> externalParameters = new();

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

            var stateTreeProp = serializedObject.FindProperty("stateTree");
            var stateTreeField = new PropertyField(stateTreeProp);

            var parameterDisplayProp = serializedObject.FindProperty("parameterDisplay");
            var parameterDisplayField = new PropertyField(parameterDisplayProp);

            var externalParametersProp = serializedObject.FindProperty("externalParameters");
            var externalParametersField = new PropertyField(externalParametersProp);

            root.AddAll(graphDataField, stateTreeField, parameterDisplayField, externalParametersField);

            if (graphDataProp.objectReferenceValue != null)
            {
                var graph = graphDataProp.objectReferenceValue as StateMachineGraph;
                injectedReferencesProp = serializedObject.FindProperty("injectedReferences");

                UpdateInjectTargets(graph);
                CreateInjectReferencesField();
            }

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
    
        private void CreateInjectReferencesField()
        {
            var entriesProp = injectedReferencesProp.FindPropertyRelative("entries");
            var entryContainer = new VisualElement();

            for (int i = 0; i < entriesProp.arraySize; i++)
            {
                var entryProp = entriesProp.GetArrayElementAtIndex(i);
                var valueProp = entryProp.FindPropertyRelative("value");

                var valueField = new PropertyField(valueProp);

                entryContainer.Add(valueField);
            }

            root.Add(entryContainer);
        }
    }
}
