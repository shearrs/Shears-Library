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

        [Header("Layers")]
        [SerializeField] private List<GraphLayer> layers = new();
        // add base elements dictionary for selection and such
        [Header("Graph Elements")]
        [SerializeReference] private GraphElementDictionary graphElements = new();
        [SerializeField] private List<string> nodeData = new();
        [SerializeField] private List<string> selection = new();
        [SerializeField] private List<string> rootNodes = new();

        private readonly List<GraphElementData> instanceSelection = new();
        private readonly List<GraphNodeData> instanceSubNodes = new();

        public Vector2 Position { get => position; set => position = value; }
        public Vector2 Scale { get => scale; set => scale = value; }

        public int SelectionCount => selection.Count;
        public IReadOnlyCollection<GraphLayer> Layers => layers;

        public event Action LayersChanged;
        public event Action<GraphNodeData> NodeDataAdded;
        public event Action<GraphNodeData> NodeDataRemoved;

        protected void AddGraphElementData(GraphElementData data)
        {
            graphElements.Add(data.ID, data);
        }

        protected void RemoveGraphElementData(GraphElementData data)
        {
            graphElements.Remove(data.ID);
        }

        #region Editor Validation
        public void Reset()
        {
            if (layers.Count == 0)
                CreateRootLayer();
        }

        public void OnValidate()
        {
            if (layers.Count == 0)
                CreateRootLayer();
        }
        #endregion

        #region Nodes
        public void SetNodePosition(GraphNodeData nodeData, Vector2 position)
        {
            nodeData.Position = position;
        }

        protected void AddNodeData(GraphNodeData data)
        {
            nodeData.Add(data.ID);
            AddGraphElementData(data);

            if (layers[^1].IsRoot())
                rootNodes.Add(data.ID);
            else
            {
                if (!TryGetData(layers[^1].ParentID, out GraphMultiNodeData parent))
                {
                    Debug.LogError("Could not get node for ID: " + layers[^1].ParentID);
                    return;
                }

                parent.AddSubNode(data);
            }

            NodeDataAdded?.Invoke(data);
        }

        protected void RemoveNodeData(GraphNodeData data)
        {
            if (!nodeData.Contains(data.ID))
            {
                Debug.LogError("Node data does not have data with ID: " + data.ID + "!");
                return;
            }
            
            nodeData.Remove(data.ID);
            RemoveGraphElementData(data);

            if (data is GraphMultiNodeData multiData)
            {
                foreach (var subNodeID in multiData.SubNodeIDs)
                {
                    if (nodeData.Contains(subNodeID) && graphElements.TryGetValue(subNodeID, out var childData))
                        RemoveNodeData((GraphNodeData)childData);
                }
            }

            if (data.ParentID == GraphLayer.ROOT_ID)
                rootNodes.Remove(data.ID);
            else
            {
                if (TryGetData(data.ParentID, out GraphMultiNodeData parent))
                    parent.RemoveSubNode(data);
            }

            NodeDataRemoved?.Invoke(data);
        }

        protected bool TryGetData<GraphElementType>(string id, out GraphElementType data) where GraphElementType : GraphElementData
        {
            data = null;

            if (!graphElements.TryGetValue(id, out var elementData))
                return false;

            data = (GraphElementType)elementData;
            return data != null;
        }
        #endregion

        #region Selection
        public void Select(GraphElementData elementData, bool isMultiSelect = false)
        {
            selection ??= new();

            if (elementData == null)
                ClearSelection();
            else
            {
                if (selection.Contains(elementData.ID))
                    return;

                if (!isMultiSelect)
                    ClearSelection();

                selection.Add(elementData.ID);
                elementData.Select();
            }
        }

        public IReadOnlyList<GraphElementData> GetSelection()
        {
            instanceSelection.Clear();

            foreach (var selectID in selection)
            {
                if (graphElements.TryGetValue(selectID, out var element))
                    instanceSelection.Add(element);
            }

            return instanceSelection;
        }

        public void DeleteSelection()
        {
            var instanceSelection = GetSelection();

            foreach (var selectable in instanceSelection)
            {
                if (selectable is GraphNodeData nodeData)
                    RemoveNodeData(nodeData);
            }

            OnDeleteSelection(instanceSelection);

            ClearSelection();
        }

        protected virtual void OnDeleteSelection(IReadOnlyList<GraphElementData> selection) { }

        private void ClearSelection()
        {
            foreach (var selectionID in selection)
            {
                if (!graphElements.TryGetValue(selectionID, out var element))
                    continue;

                element.Deselect();
            }

            selection.Clear();
        }
        #endregion

        #region Layers
        public void OpenLayer(GraphLayer layer)
        {
            if (layer.IsRoot())
                OpenRootLayer();
            else if (TryGetData<GraphMultiNodeData>(layer.ParentID, out var parentNode))
                OpenMultiNode(parentNode);
            else
                Debug.LogError("Could not find node for layer: " + layer.ParentID);
        }

        private void OpenRootLayer()
        {
            if (layers.Count > 0 && layers[^1].IsRoot())
                return;

            ClearLayers();
            CreateRootLayer();

            LayersChanged?.Invoke();
        }

        private void OpenMultiNode(GraphMultiNodeData node)
        {
            if (layers.Count > 1 && layers[^1].ParentID == node.ID)
                return;

            ClearLayers();
            CreateRootLayer();
            layers.Add(CreateLayer(node));

            string parentID = node.ParentID;

            while (parentID != null && parentID != string.Empty)
            {
                if (!nodeData.Contains(parentID) || !graphElements.TryGetValue(parentID, out var untypedParent))
                    break;

                if (untypedParent == null || untypedParent is not GraphMultiNodeData parent)
                    break;

                layers.Insert(1, CreateLayer(parent));
                parentID = parent.ParentID;
            }

            LayersChanged?.Invoke();
        }

        public IReadOnlyCollection<GraphNodeData> GetActiveNodes()
        {
            return GetLayerSubNodes(layers[^1]); 
        }

        private IReadOnlyList<GraphNodeData> GetLayerSubNodes(GraphLayer layer)
        {
            instanceSubNodes.Clear();

            TryGetData(layer.ParentID, out GraphMultiNodeData multiNode);

            IReadOnlyList<string> nodeIDs;

            if (multiNode == null)
                nodeIDs = rootNodes;
            else
                nodeIDs = multiNode.SubNodeIDs;

            foreach (var nodeID in nodeIDs)
            {
                if (!TryGetData(nodeID, out GraphNodeData node))
                    continue;

                instanceSubNodes.Add(node);
            }

            return instanceSubNodes;
        }

        private void CreateRootLayer()
        {
            var layer = CreateLayer(Vector2.zero, Vector2.one, null);
            layers.Add(layer);
        }
        private GraphLayer CreateLayer(GraphMultiNodeData parent) => CreateLayer(Vector2.zero, Vector2.one, parent);
        private GraphLayer CreateLayer(Vector2 position, Vector2 scale, GraphMultiNodeData parent)
        {
            return new GraphLayer(position, scale, parent);
        }

        private void ClearLayers()
        {
            layers.Clear();
        }
        #endregion Layers
    }
}
