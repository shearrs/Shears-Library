using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.GraphViews
{
    public abstract class GraphData : ScriptableObject
    {
        [Header("Transform")]
        [SerializeField] private Vector2 position;
        [SerializeField] private Vector2 scale = Vector2.one;

        [Header("Graph Elements")]
        [SerializeReference] private GraphSelectionDictionary selection = new();
        [SerializeReference] private GraphNodeDictionary nodeData = new();
        [SerializeReference] private GraphEdgeDictionary edgeData = new();
        [SerializeReference] private List<GraphNodeData> rootNodes = new();
        [SerializeReference] private List<GraphMultiNodeData> nodePath = new();
        
        public Vector2 Position { get => position; set => position = value; }
        public Vector2 Scale { get => scale; set => scale = value; }

        public IReadOnlyCollection<GraphElementData> Selection => selection.Values;
        public IReadOnlyCollection<GraphMultiNodeData> NodePath => nodePath;

        public event Action NodePathChanged;

        public void SetNodePosition(GraphNodeData nodeData, Vector2 position)
        {
            nodeData.Position = position;
        }

        public void OpenRootPath()
        {
            if (nodePath.Count == 0)
                return;

            nodePath.Clear();

            NodePathChanged?.Invoke();
        }

        public void OpenSubPath(GraphMultiNodeData path)
        {
            if (nodePath.Count > 0 && nodePath[^1] == path)
                return;

            nodePath.Clear();
            nodePath.Add(path);

            string parentID = path.ParentID;

            while (parentID != null && parentID != string.Empty)
            {
                nodeData.TryGetValue(parentID, out var untypedParent);

                if (untypedParent == null || untypedParent is not GraphMultiNodeData parent)
                    break;

                nodePath.Insert(0, parent);
                parentID = parent.ParentID;
            }

            NodePathChanged?.Invoke();
        }

        public IReadOnlyCollection<GraphNodeData> GetActiveNodes()
        {
            if (nodePath.Count > 0)
                return nodePath[^1].GetSubNodes();

            return rootNodes;
        }

        public void Select(GraphElementData elementData, bool isMultiSelect = false)
        {
            selection ??= new();

            if (elementData == null)
                ClearSelection();
            else
            {
                if (selection.ContainsKey(elementData.ID))
                    return;

                if (!isMultiSelect)
                    ClearSelection();

                selection.TryAdd(elementData.ID, elementData);
                elementData.Select();
            }
        }

        protected void AddNodeData(GraphNodeData data)
        {
            nodeData.Add(data.ID, data);

            if (nodePath.Count == 0)
                rootNodes.Add(data);
            else
                nodePath[^1].AddSubNode(data);
        }

        private void ClearSelection()
        {
            foreach (var selectedElement in selection.Values)
                selectedElement.Deselect();

            selection.Clear();
        }
    }
}
