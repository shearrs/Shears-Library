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

            var scriptField = VisualElementUtil.CreateScriptField(serializedObject);
            var gridSizeField = new PropertyField(gridSizeProp);
            var nodeSizeField = new PropertyField(nodeSizeProp);
            var nodesField = new PropertyField(nodesProp);

            gridSizeField.RegisterValueChangeCallback((evt) => OnGridChanged());
            nodeSizeField.RegisterValueChangeCallback((evt) => OnGridChanged());
            nodesField.RegisterValueChangeCallback((evt) => OnGridChanged());

            root.AddAll(scriptField, gridSizeField, nodeSizeField, nodesField);

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

            // destroy objects
            // it would be nice if we didnt just clear this and instead we just removed things if they needed to be shrunk, or added things if they needed to be added
            // decreasing => delete extras
            // increasing => add new ones, we can only grow outwards to the right and up so it should be easy

            List<PathNode> newNodes = new();
            for (int z = 0; z < newGridSize.z; z++)
            {
                for (int y = 0; y < newGridSize.y; y++)
                {
                    for (int x = 0; x < newGridSize.x; x++)
                    {
                        // check if this node existed in the previous grid
                        if (x < previousGridSize.x && y < previousGridSize.y && z < previousGridSize.z)
                        {
                            int oldIndex = (z * previousGridSize.y * previousGridSize.x) + (y * previousGridSize.x) + x; ;
                            var existingNode = nodesProp.GetArrayElementAtIndex(oldIndex).boxedValue as PathNode;

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

            nodesProp.ClearArray();

            for (int i = 0; i < newNodes.Count; i++)
            {
                nodesProp.InsertArrayElementAtIndex(nodesProp.arraySize);
                nodesProp.GetArrayElementAtIndex(nodesProp.arraySize - 1).boxedValue = newNodes[i];
            }

            //for (int z = 0; z < newGridSize.z; z++)
            //{
            //    for (int y = 0; y < newGridSize.y; y++)
            //    {
            //        for (int x = 0; x < newGridSize.x; x++)
            //        {
            //            Vector3 localPosition = new(
            //                x * nodeSize,
            //                y * nodeSize,
            //                z * nodeSize
            //            );

            //            var node = new PathNode(new(x, y, z), grid.transform.TransformPoint(localPosition));
            //            nodesProp.InsertArrayElementAtIndex(nodesProp.arraySize);
            //            nodesProp.GetArrayElementAtIndex(nodesProp.arraySize - 1).boxedValue = node;
            //        }
            //    }
            //}

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
