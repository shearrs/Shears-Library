using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Grids
{
    [Serializable]
    public class GridNodeDataCache
    {
        [SerializeReference]
        private List<GridNodeData> data = new();

#if UNITY_EDITOR
        internal Color EditorColor
        {
            get
            {
                if (data.Count > 0)
                    return data[0].InternalEditorColor;
                else
                    return GridNodeData.DEFAULT_EDITOR_COLOR;
            }
        }
#endif

        public int DataCount => data.Count;

        public GridNodeDataCache() { }

        public void Clear()
        {
            data.Clear();
        }

        public void Dispose()
        {
            foreach (var nodeData in data)
                nodeData.Dispose();
        }

        public void Copy(GridNodeDataCache cache)
        {
            data.AddRange(cache.data);
        }

        public T AddData<T>()
            where T : GridNodeData, new()
        {
            var newData = new T();

            data.Add(newData);

            return newData;
        }

        public bool RemoveData<T>()
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i] is T)
                {
                    data.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public bool TryGetData<T>(out T targetData)
            where T : GridNodeData
        {
            foreach (var nodeData in data)
            {
                if (nodeData is T typedData)
                {
                    targetData = typedData;
                    return true;
                }
            }

            targetData = null;
            return false;
        }

        public bool TryGetData(Type type, out GridNodeData targetData)
        {
            foreach (var nodeData in data)
            {
                if (nodeData.GetType() == type)
                {
                    targetData = nodeData;
                    return true;
                }
            }

            targetData = null;
            return false;
        }

        internal bool TryGetAutomaticEditorColor(out Color color)
        {
            color = Color.magenta;

            if (DataCount == 0)
                return false;

            foreach (var nodeData in data)
            {
                if (nodeData.InternalEditorAutomaticTarget)
                {
                    color = nodeData.InternalEditorColor;
                    return true;
                }
            }

            return false;
        }
    }
}
