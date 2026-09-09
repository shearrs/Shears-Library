using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shears.Grids
{
    [Serializable]
    public class GridNode
    {
#if UNITY_EDITOR
        [SerializeField]
        private GridNodeDefinition definition;
#endif

        [SerializeField]
        private NodeObject nodeObject;

        [SerializeField]
        private Vector3Int gridPosition;

        [SerializeField]
        private GridNodeDataCache data = new();

        public Color EditorColor => data.EditorColor;
        public GridNodeDefinition Definition
        {
            get
            {
#if UNITY_EDITOR
                return definition;
#else
                return null;
#endif
            }
        }
        public NodeObject NodeObject
        {
            get => nodeObject;
            set => nodeObject = value;
        }
        public Vector3Int GridPosition
        {
            get => gridPosition;
            internal set => SetGridPosition(value);
        }
        public int DataCount => data.DataCount;

        public event Action<NodeUpdateData> Updated;

        public readonly struct NodeUpdateData
        {
            public GridNode Node { get; }
            public GridNodeData Data { get; }

            public NodeUpdateData(GridNode node, GridNodeData data)
            {
                Node = node;
                Data = data;
            }
        }

        public GridNode() { }

        public GridNode(GridNodeDefinition definition)
        {
            SetDefinition(definition);
        }

        public GridNode(Vector3Int gridPosition)
        {
            this.gridPosition = gridPosition;
        }

        public GridNode(int x, int y, int z)
        {
            gridPosition = new(x, y, z);
        }

        public void Dispose()
        {
            data.Dispose();
        }

        public T AddData<T>()
            where T : GridNodeData, new()
        {
            var newData = data.AddData<T>();

            newData.Updated += OnDataUpdated;

            return newData;
        }

        public bool RemoveData<T>()
            where T : GridNodeData
        {
            if (!data.TryGetData<T>(out var targetData))
                return false;

            targetData.Updated -= OnDataUpdated;

            return data.RemoveData(targetData);
        }

        public bool TryGetData<T>(out T targetData)
            where T : GridNodeData => data.TryGetData(out targetData);

        public bool TryGetAutomaticEditorColor(out Color color)
        {
#if UNITY_EDITOR
            if (data.TryGetAutomaticEditorColor(out color))
                return true;
            else if (definition != null)
                return definition.Data.TryGetAutomaticEditorColor(out color);
            else
                return false;
#else
            color = default;
            return false;
#endif
        }

        public void SetDefinition(GridNodeDefinition definition)
        {
#if UNITY_EDITOR
            if (this.definition == definition)
                return;

            data.Clear();
            this.definition = definition;

            if (definition != null)
                data.Copy(definition.Data);

            if (nodeObject != null)
                UnityEditor.Undo.DestroyObjectImmediate(nodeObject.gameObject);
#endif
        }

        public void CreateNodeObject(ShearsGrid grid)
        {
#if UNITY_EDITOR
            if (nodeObject != null)
                Object.DestroyImmediate(nodeObject.gameObject);

            if (definition == null || definition.NodeObject == null)
                return;

            var instance =
                UnityEditor.PrefabUtility.InstantiatePrefab(definition.NodeObject, grid.transform)
                as NodeObject;

            instance.Grid = grid;
            instance.GridPosition = gridPosition;

            var position = grid.GetLocalPosition(this);

            instance.transform.SetLocalPositionAndRotation(
                grid.GetLocalPosition(this),
                Quaternion.identity
            );

            UnityEditor.Undo.RegisterCreatedObjectUndo(instance, "Create Node Object");

            nodeObject = instance;
#endif
        }

        public void DestroyNodeObject()
        {
            if (NodeObject == null)
                return;

            if (!Application.isPlaying)
            {
#if UNITY_EDITOR
                UnityEditor.Undo.DestroyObjectImmediate(NodeObject.gameObject);
#endif
            }
            else
                Object.Destroy(NodeObject.gameObject);
        }

        public void SyncDefinition(ShearsGrid grid)
        {
#if UNITY_EDITOR
            data.Clear();

            if (definition == null)
                return;

            data.Copy(definition.Data);

            if (nodeObject == null && definition.NodeObject != null)
                CreateNodeObject(grid);
            else if (nodeObject != null && definition.NodeObject == null)
                UnityEditor.Undo.DestroyObjectImmediate(nodeObject.gameObject);
            else if (definition.NodeObject != null)
            {
                string currentPath =
                    UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(nodeObject);
                string targetPath =
                    UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                        definition.NodeObject
                    );

                if (currentPath != targetPath)
                    CreateNodeObject(grid);
            }
#endif
        }

        public GridNode Clone()
        {
            var clone = new GridNode(gridPosition);
            clone.data.Copy(data);
            clone.nodeObject = nodeObject;

            return clone;
        }

        public void DrawHandles(ShearsGrid grid)
        {
            var context = new GridNodeHandleContext(grid, GridPosition, grid.NodeSize);

            foreach (var nodeData in data.Data)
                nodeData.DrawHandles(context);
        }

        private void SetGridPosition(Vector3Int gridPosition)
        {
            this.gridPosition = gridPosition;

            if (nodeObject != null)
                nodeObject.GridPosition = gridPosition;
        }

        private void OnDataUpdated(GridNodeData data)
        {
            Updated?.Invoke(new(this, data));
        }
    }
}
