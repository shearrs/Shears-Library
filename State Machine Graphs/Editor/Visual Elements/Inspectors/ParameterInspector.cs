using Shears.Editor;
using Shears.GraphViews;
using Shears.GraphViews.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class ParameterInspector : VisualElement
    {
        private readonly StateMachineGraph graphData;

        private ParameterData parameterData;
        private SerializedProperty parameterProp;

        public ParameterInspector(StateMachineGraph graphData)
        {
            this.graphData = graphData;

            AddToClassList(SMEditorUtil.ParameterInspectorClassName);
        }

        public void SetParameter(ParameterData data)
        {
            Clear();

            parameterData = data;
            parameterProp = GraphViewEditorUtil.GetElementProp(graphData, parameterData.ID);

            var typeField = CreateTypeField();
            var nameField = CreateNameField();

            this.AddAll(typeField, nameField);
        }

        private VisualElement CreateTypeField()
        {
            var typeName = StringUtil.PascalSpace(parameterData.GetType().Name);
            var typeField = Shears.Editor.VisualElementUtil.CreateHeader(typeName);

            typeField.style.marginTop = 2;
            typeField.style.marginBottom = 4;

            return typeField;
        }

        private VisualElement CreateNameField()
        {
            var nameProp = parameterProp.FindPropertyRelative("name");

            var nameField = new TextField("Name");

            nameField.BindProperty(nameProp);
            nameField.AddToClassList(SMEditorUtil.NodeTitleInspectorClassName);

            return nameField;
        }
    }
}
