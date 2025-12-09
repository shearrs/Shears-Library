using UnityEditor;
using UnityEngine;

namespace Shears.Pathfinding.Editor
{
    public class PGETPainter
    {
        private readonly PGETSettings settings;

        public PGETPainter(PGETSettings settings)
        {
            this.settings = settings;
        }

        public void PaintNode(PathNode node, SerializedProperty nodeProp)
        {
            if (settings.DrawNodeData && (node.Data != null || settings.NodeData != null))
            {
                var dataProp = nodeProp.FindPropertyRelative("data");

                if (settings.NodeData == null)
                    dataProp.boxedValue = null;
                else
                    dataProp.boxedValue = settings.NodeData.Clone();
            }

            if (settings.DrawPrefab)
                PaintPrefab(node, nodeProp);
        }

        private void PaintPrefab(PathNode node, SerializedProperty nodeProp)
        {
            var nodeObjectProp = nodeProp.FindPropertyRelative("nodeObject");

            if (nodeObjectProp.objectReferenceValue != null)
                Undo.DestroyObjectImmediate(nodeObjectProp.objectReferenceValue);

            if (settings.NodePrefab == null) // if we are placing nothing, just clear the object value
            {
                nodeObjectProp.objectReferenceValue = null;
                return;
            }

            var newInstance = PrefabUtility.InstantiatePrefab(settings.NodePrefab, settings.Grid.transform) as GameObject;
            Undo.RegisterCreatedObjectUndo(newInstance, "Instantiate Node Prefab");
            newInstance.transform.position = PathGridEditorTool.GetWorldPosition(settings.Grid, node);

            nodeObjectProp.objectReferenceValue = newInstance;
        }
    }
}
