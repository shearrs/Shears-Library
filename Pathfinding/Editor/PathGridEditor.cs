using Shears.Editor;
using System.Collections.Generic;
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
        private Vector3Int previousGridSize;
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

            previousGridSize = gridSizeProp.vector3IntValue;

            var scriptField = Shears.Editor.VisualElementUtil.CreateScriptField(serializedObject);
            var gridSizeField = new PropertyField(gridSizeProp);
            var nodeSizeField = new PropertyField(nodeSizeProp);

            gridSizeField.RegisterValueChangeCallback((evt) => OnGridChanged());
            nodeSizeField.RegisterValueChangeCallback((evt) => OnGridChanged());

            root.AddAll(scriptField, gridSizeField, nodeSizeField);

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

            Vector3Int newGridSize = gridSizeProp.vector3IntValue;
            float nodeSize = nodeSizeProp.floatValue;

            // array already initialized
            if (nodesProp.arraySize == newGridSize.x * newGridSize.y * newGridSize.z)
                return;

            bool isInitialized = nodesProp.arraySize > 0;

            // destroy objects
            // it would be nice if we didnt just clear this and instead we just removed things if they needed to be shrunk, or added things if they needed to be added
            // decreasing => delete extras
            // increasing => add new ones, we can only grow outwards to the right and up so it should be easy
            HashSet<PathNodeObject> reusedObjects = new();
            List<PathNode> newNodes = new();

            for (int z = 0; z < newGridSize.z; z++)
            {
                for (int y = 0; y < newGridSize.y; y++)
                {
                    for (int x = 0; x < newGridSize.x; x++)
                    {
                        // check if this node existed in the previous grid
                        if (isInitialized && x < previousGridSize.x && y < previousGridSize.y && z < previousGridSize.z)
                        {
                            int oldIndex = (z * previousGridSize.y * previousGridSize.x) + (y * previousGridSize.x) + x;
                            var existingNodeProp = nodesProp.GetArrayElementAtIndex(oldIndex);
                            var existingNode = existingNodeProp.boxedValue as PathNode;

                            if (existingNode.NodeObject != null)
                                reusedObjects.Add(existingNode.NodeObject);

                            newNodes.Add(existingNode);
                        }
                        else // create new node
                        {
                            Vector3 localPosition = new(
                                x * nodeSize,
                                y * nodeSize,
                                z * nodeSize
                            );

                            var newNode = new PathNode(new(x, y, z), grid.transform.TransformPoint(localPosition));
                            newNodes.Add(newNode);
                        }
                    }
                }
            }

            for (int i = 0; i < nodesProp.arraySize; i++)
            {
                var element = nodesProp.GetArrayElementAtIndex(i);
                var node = element.boxedValue as PathNode;

                if (node.NodeObject != null && !reusedObjects.Contains(node.NodeObject))
                    Undo.DestroyObjectImmediate(node.NodeObject.gameObject);
            }

            nodesProp.ClearArray();

            for (int i = 0; i < newNodes.Count; i++)
            {
                nodesProp.InsertArrayElementAtIndex(nodesProp.arraySize);
                var newNodeProp = nodesProp.GetArrayElementAtIndex(nodesProp.arraySize - 1);
                newNodeProp.boxedValue = newNodes[i];

                var sizeProp = newNodeProp.FindPropertyRelative("size");
                sizeProp.floatValue = grid.NodeSize;
            }

            previousGridSize = newGridSize;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        [MenuItem("CONTEXT/PathGrid/Update World Position")]
        private static void UpdateWorldPositions(MenuCommand command)
        {
            var grid = command.context as PathGrid;

            grid.UpdateWorldPositions();
        }
    }
}
