using Shears.GraphViews.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    [CustomPropertyDrawer(typeof(TransitionEdgeData))]
    public class TransitionEdgeDataPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.AddStyleSheet(SMEditorUtil.SMGraphInspectorStyleSheet);
            root.AddToClassList(SMEditorUtil.TransitionClassName);

            root.Add(CreateTitle(property));
            root.Add(CreateComparisonList(property));

            return root;
        }

        private VisualElement CreateTitle(SerializedProperty transitionProp)
        {
            var graphSO = transitionProp.serializedObject;
            var fromID = transitionProp.FindPropertyRelative("fromID").stringValue;
            var toID = transitionProp.FindPropertyRelative("toID").stringValue;

            var fromProp = GraphViewEditorUtil.GetElementProp(graphSO, fromID);
            var toProp = GraphViewEditorUtil.GetElementProp(graphSO, toID);
            var fromNameProp = fromProp.FindPropertyRelative("name");
            var toNameProp = toProp.FindPropertyRelative("name");

            var title = new VisualElement();
            title.AddToClassList(SMEditorUtil.TransitionTitleClassName);

            var fromLabel = new Label();
            var symbolLabel = new Label(" -> ");
            var toLabel = new Label();

            fromLabel.BindProperty(fromNameProp);
            toLabel.BindProperty(toNameProp);

            title.AddAll(fromLabel, symbolLabel, toLabel);

            return title;
        }

        private VisualElement CreateComparisonList(SerializedProperty transitionProp)
        {
            var comparisonsProp = transitionProp.FindPropertyRelative("comparisonData");
            var comparisonProps = new List<SerializedProperty>();

            for (int i = 0; i < comparisonsProp.arraySize; ++i)
                comparisonProps.Add(comparisonsProp.GetArrayElementAtIndex(i));

            VisualElement makeItem() => new PropertyField();
            void bindItem(VisualElement e, int i) => (e as PropertyField).BindProperty(comparisonProps[i]);

            var listView = new ListView(comparisonProps, makeItem: makeItem, bindItem: bindItem);

            return listView;
        }
    }
}
