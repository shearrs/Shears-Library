using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachines.Editor
{
    [CustomEditor(typeof(StateMachineBase<>), true)]
    public class StateMachineEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.styleSheets.Add(Resources.Load<StyleSheet>("StateMachines"));

            var scriptField = new PropertyField(serializedObject.FindProperty("m_Script"));
            scriptField.SetEnabled(false);

            var initialStateField = new PropertyField(serializedObject.FindProperty("initialState"));
            var stateTreeField = new PropertyField(serializedObject.FindProperty("stateDisplay"));

            var parametersHeader = new Label("Parameters");
            parametersHeader.AddToClassList("header");

            var addParameterButton = new Button(OpenParameterContextMenu)
            {
                text = "Add Parameter"
            };

            var parametersField = new PropertyField(serializedObject.FindProperty("parameters"));

            root.Add(scriptField);
            root.Add(initialStateField);
            root.Add(stateTreeField);
            root.Add(parametersHeader);
            root.Add(addParameterButton);
            root.Add(parametersField);

            return root;
        }

        private void OpenParameterContextMenu()
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Primitive/Bool Parameter"), false, () => AddParameter(new BoolParameter("Bool Parameter")));
            menu.AddItem(new GUIContent("Primitive/Int Parameter"), false, () => AddParameter(new IntParameter("Int Parameter")));
            menu.AddItem(new GUIContent("Primitive/Float Parameter"), false, () => AddParameter(new FloatParameter("Float Parameter")));
            menu.AddItem(new GUIContent("Composite/Trigger Parameter"), false, () => AddParameter(new TriggerParameter("Trigger Parameter")));
            menu.AddItem(new GUIContent("Composite/Vector2 Parameter"), false, () => AddParameter(new Vector2Parameter("Vector2 Parameter")));
            menu.AddItem(new GUIContent("Composite/Object Parameter"), false, () => AddParameter(new ObjectParameter("Object Parameter")));

            menu.ShowAsContext();
        }

        private void AddParameter(Parameter parameter)
        {
            var parametersProperty = serializedObject.FindProperty("parameters");

            parametersProperty.InsertArrayElementAtIndex(parametersProperty.arraySize);
            var newParameterProperty = parametersProperty.GetArrayElementAtIndex(parametersProperty.arraySize - 1);
            newParameterProperty.managedReferenceValue = parameter;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
}
