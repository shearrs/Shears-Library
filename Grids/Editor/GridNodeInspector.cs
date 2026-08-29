using Shears.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Grids.Editor
{
    public class GridNodeInspector : VisualElement
    {
        public GridNodeInspector(SerializedProperty nodeProp, int nodeIndex)
        {
            AddToClassList(ShearsGridStyles.NodeInspectorClass);
            var gridPositionProp = nodeProp.FindPropertyRelative("gridPosition");
            var definitionProp = nodeProp.FindPropertyRelative("definition");
            var nodeObjectProp = nodeProp.FindPropertyRelative("nodeObject");

            var label = new Label($"Node {nodeIndex}")
            {
                name = "Node Header",
                style = { unityFontStyleAndWeight = FontStyle.Bold },
            };
            label.AddBaseFieldLabelClass();

            var gridPositionField = new PropertyField();
            gridPositionField.BindProperty(gridPositionProp);
            gridPositionField.SetEnabled(false);

            var definitionField = new PropertyField();
            definitionField.BindProperty(definitionProp);
            definitionField.SetEnabled(false);

            var nodeObjectField = new PropertyField();
            nodeObjectField.BindProperty(nodeObjectProp);
            nodeObjectField.SetEnabled(false);

            this.AddAll(label, gridPositionField, definitionField, nodeObjectField);
        }
    }
}
