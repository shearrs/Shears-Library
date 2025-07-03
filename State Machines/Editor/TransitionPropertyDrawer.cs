using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachines.Editor
{
    [CustomPropertyDrawer(typeof(Transition))]
    public class TransitionPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.styleSheets.Add(Resources.Load<StyleSheet>("StateMachines"));

            // States
            var statesHeader = new Label("States");
            statesHeader.AddToClassList("header");

            var fromField = new PropertyField(property.FindPropertyRelative("from"));
            var toField = CreateToField(property);

            // Comparisons
            var comparisonsHeader = new Label("Parameter Comparisons");
            comparisonsHeader.AddToClassList("header");

            var addComparisonButton = new Button(() => OpenComparisonContextMenu(property))
            {
                text = "Add Comparison"
            };

            var comparisonsField = new PropertyField(property.FindPropertyRelative("comparisons"));

            root.Add(statesHeader);
            root.Add(fromField);
            root.Add(toField);
            root.Add(comparisonsHeader);
            root.Add(addComparisonButton);
            root.Add(comparisonsField);

            return root;
        }

        private Foldout CreateFoldout(SerializedProperty property)
        {
            string toState = GetStateName(property);

            var foldout = new Foldout
            {
                text = toState,
                value = property.isExpanded
            };

            foldout.RegisterValueChangedCallback(evt =>
            {
                property.isExpanded = evt.newValue;
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            });

            return foldout;
        }

        private VisualElement CreateToField(SerializedProperty property)
        {
            var toField = new PropertyField(property.FindPropertyRelative("to"));

            return toField;
        }

        private string GetStateName(SerializedProperty property)
        {
            var to = property.FindPropertyRelative("to").objectReferenceValue;

            return to != null
                ? to.GetType().Name
                : "None";
        }

        private void OpenComparisonContextMenu(SerializedProperty property)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Primitive/Bool Comparison"), false, () => AddComparison(property, new BoolParameterComparison()));
            menu.AddItem(new GUIContent("Primitive/Int Comparison"), false, () => AddComparison(property, new IntParameterComparison()));
            menu.AddItem(new GUIContent("Primitive/Float Comparison"), false, () => AddComparison(property, new FloatParameterComparison()));
            menu.AddItem(new GUIContent("Composite/Trigger Comparison"), false, () => AddComparison(property, new TriggerParameterComparison()));
            menu.AddItem(new GUIContent("Composite/Object Comparison"), false, () => AddComparison(property, new ObjectParameterComparison()));

            menu.ShowAsContext();
        }

        private void AddComparison(SerializedProperty property, ParameterComparison comparison)
        {
            var comparisonsProperty = property.FindPropertyRelative("comparisons");

            comparisonsProperty.InsertArrayElementAtIndex(comparisonsProperty.arraySize);
            var newComparisonProperty = comparisonsProperty.GetArrayElementAtIndex(comparisonsProperty.arraySize - 1);
            newComparisonProperty.managedReferenceValue = comparison;

            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
            AssetDatabase.SaveAssets();
        }
    }
}
