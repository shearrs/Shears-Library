using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Pathfinding.Editor
{
    [CustomEditor(typeof(PathGrid))]
    public class PathGridEditor : UnityEditor.Editor
    {
        private PathGrid grid;
        private SerializedProperty gridSizeProp;
        private SerializedProperty nodeSizeProp;
        private SerializedProperty nodesProp;

        private void OnEnable()
        {
            grid = serializedObject.targetObject as PathGrid;

            if (SerializationUtility.HasManagedReferencesWithMissingTypes(grid))
                SerializationUtility.ClearAllManagedReferencesWithMissingTypes(grid);
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            grid = serializedObject.targetObject as PathGrid;
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
            for (int i = 0; i < nodesProp.arraySize; i++)
            {
                var element = nodesProp.GetArrayElementAtIndex(i);

                if (element.boxedValue == null)
                {
                    nodesProp.DeleteArrayElementAtIndex(i);
                    i--;
                }
            }

            Vector3Int gridSize = gridSizeProp.vector3IntValue;
            float nodeSize = nodeSizeProp.floatValue;

            // array already initialized
            if (nodesProp.arraySize == gridSize.x * gridSize.y * gridSize.z)
                return;

            nodesProp.ClearArray();

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

                        var node = new PathNode(new(x, y, z), grid.transform.TransformPoint(localPosition));
                        nodesProp.InsertArrayElementAtIndex(nodesProp.arraySize);
                        nodesProp.GetArrayElementAtIndex(nodesProp.arraySize - 1).boxedValue = node;
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("CONTEXT/PathGrid/Update World Position")]
        private static void UpdateWorldPositions(MenuCommand command)
        {
            var grid = command.context as PathGrid;

            grid.UpdateWorldPositions();
        }
    }
}
