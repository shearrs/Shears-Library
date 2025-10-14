using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Pathfinding.Editor
{
    [CustomEditor(typeof(PathGrid))]
    public class PathGridEditor : UnityEditor.Editor
    {
        private SerializedProperty gridSizeProp;
        private SerializedProperty nodeSizeProp;
        private SerializedProperty nodesProp;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            gridSizeProp = serializedObject.FindProperty("gridSize");
            nodeSizeProp = serializedObject.FindProperty("nodeSize");
            nodesProp = serializedObject.FindProperty("nodes");

            var gridSizeField = new PropertyField(gridSizeProp);
            var nodeSizeField = new PropertyField(nodeSizeProp);
            var nodesField = new PropertyField(nodesProp);

            gridSizeField.RegisterValueChangeCallback((evt) => OnGridChanged());
            nodeSizeField.RegisterValueChangeCallback((evt) => OnGridChanged());
            nodesField.RegisterValueChangeCallback((evt) => OnGridChanged());

            root.AddAll(gridSizeField, nodeSizeField, nodesField);

            return root;
        }

        private void OnGridChanged()
        {
            nodesProp.ClearArray();

            Vector3 gridSize = gridSizeProp.vector3IntValue;
            float nodeSize = nodeSizeProp.floatValue;

            for (int z = 0; z < gridSize.z; z++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    for (int x = 0; x < gridSize.x; x++)
                    {
                        Vector3 localPosition = new(
                            x * nodeSize,
                            y * nodeSize,
                            z * nodeSize
                        );

                        var node = new PathNode(new(x, y, z), localPosition);
                        nodesProp.InsertArrayElementAtIndex(nodesProp.arraySize);
                        nodesProp.GetArrayElementAtIndex(nodesProp.arraySize - 1).boxedValue = node;
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
