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

                UpdateInjectTargets(graph);

                injectReferencesProp = serializedObject.FindProperty("injectReferences");
                var injectReferencesField = new PropertyField(injectReferencesProp);
                root.Add(injectReferencesField);
            }

            return root;
        }

        private void UpdateInjectTargets(StateMachineGraph graph)
        {
            var states = graph.GetStateNodes();

            foreach (var state in states)
            {
                var type = state.StateType.SystemType;
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                Debug.Log("state: " + type.Name);

                foreach (var field in fields)
                {
                    if (!field.IsDefined(typeof(StateInjectAttribute), false))
                        continue;

                    Debug.Log("field: " + field.Name);

                    var fieldType = field.FieldType;

                    if (stateMachine.HasInjectedReference(state.ID, fieldType))
                        continue;

                    // inject a StateInjectReference with this state id, the field type, and it needs a property drawer to draw an object field of the correct type
                    var injectReference = new StateInjectReference(state.ID, fieldType);

                    injectReferencesProp.InsertArrayElementAtIndex(injectReferencesProp.arraySize);
                    var element = injectReferencesProp.GetArrayElementAtIndex(injectReferencesProp.arraySize - 1);

                    element.boxedValue = injectReference;
                }
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}
