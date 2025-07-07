using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shears.GraphViews
{
    public abstract class GraphData : ScriptableObject
    {
        [Header("Transform")]
        [SerializeField] private Vector2 position;
        [SerializeField] private Vector2 scale = Vector2.one;

        [Header("Graph Elements")]
        [SerializeReference] private GraphNodeDictionary nodeData = new();
        [SerializeField] private List<string> selection = new();
        [SerializeField] private List<string> rootNodes = new();
        [SerializeField] private List<GraphLayer> layers = new();

        private readonly List<GraphElementData> instanceSelection = new();
        private readonly List<GraphNodeData> instanceSubNodes = new();

        public Vector2 Position { get => position; set => position = value; }
        public Vector2 Scale { get => scale; set => scale = value; }
        
        public IReadOnlyCollection<GraphLayer> Layers => layers;

        public event Action LayersChanged;

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

        public void SetNodePosition(GraphNodeData nodeData, Vector2 position)
        {
            nodeData.Position = position;
        }

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
                instanceSelection.Add(nodeData[selectID]);

            return instanceSelection;
        }

        private void ClearSelection()
        {
            foreach (var selectionID in selection)
            {
                if (!nodeData.TryGetValue(selectionID, out var node))
                    continue;

                node.Deselect();
            }

            selection.Clear();
        }
        #endregion

        protected void AddNodeData(GraphNodeData data)
        {
            nodeData.Add(data.ID, data);

            if (layers[^1].IsRoot())
                rootNodes.Add(data.ID);
            else
            {
                if (!TryGetNode(layers[^1].ParentID, out GraphMultiNodeData parent))
                {
                    Debug.LogError("Could not get node for ID: " + layers[^1].ParentID);
                    return;
                }

                parent.AddSubNode(data);
            }
        }

        protected bool TryGetNode<NodeDataType>(string id, out NodeDataType data) where NodeDataType : GraphNodeData
        {
            data = null;

            if (!nodeData.TryGetValue(id, out var value))
                return false;

            data = (NodeDataType)value;
            return data != null;
        }

        #region Layers
        public void OpenLayer(GraphLayer layer)
        {
            if (layer.IsRoot())
                OpenRootLayer();
            else if (TryGetNode<GraphMultiNodeData>(layer.ParentID, out var parentNode))
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
                nodeData.TryGetValue(parentID, out var untypedParent);

                if (untypedParent == null || untypedParent is not GraphMultiNodeData parent)
                    break;

                layers.Insert(1, CreateLayer(parent));
                parentID = parent.ParentID;
            }

            LayersChanged?.Invoke();
        }

        public IReadOnlyCollection<GraphNodeData> GetActiveNodes()
        {
            Debug.Log("subnodes: " + GetLayerSubNodes(layers[^1]).Count);

            return GetLayerSubNodes(layers[^1]); 
        }

        private IReadOnlyList<GraphNodeData> GetLayerSubNodes(GraphLayer layer)
        {
            instanceSubNodes.Clear();

            TryGetNode(layer.ParentID, out GraphMultiNodeData multiNode);

            IReadOnlyList<string> nodeIDs;

            if (multiNode == null)
                nodeIDs = rootNodes;
            else
                nodeIDs = multiNode.SubNodeIDs;

            foreach (var nodeID in nodeIDs)
            {
                if (!TryGetNode(nodeID, out GraphNodeData node))
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
