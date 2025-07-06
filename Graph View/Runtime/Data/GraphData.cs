using System.Collections.Generic;
using UnityEngine;

namespace Shears.GraphViews
{
    public abstract class GraphData : ScriptableObject
    {
        [Header("Transform")]
        [SerializeField] private Vector2 position;
        [SerializeField] private Vector2 scale = Vector2.one;

        [Header("Elements")]
        [SerializeReference] private GraphSelectionDictionary selection = new();
        [SerializeReference] private GraphNodeDictionary nodeData = new();
        [SerializeReference] private GraphEdgeDictionary edgeData = new();
        [SerializeReference] private Stack<GraphNodeData> nodePath = new();
        
        public Vector2 Position { get => position; set => position = value; }
        public Vector2 Scale { get => scale; set => scale = value; }

        public IReadOnlyCollection<GraphElementData> Selection => selection.Values;
        public IReadOnlyCollection<GraphNodeData> NodeData => nodeData.Values;
        public IReadOnlyCollection<GraphEdgeData> EdgeData => edgeData.Values;
        public IReadOnlyCollection<GraphNodeData> NodePath => nodePath;

        protected void AddNodeData(GraphNodeData data)
        {
            nodeData.Add(data.ID, data);
        }

        public void Select(GraphElementData elementData)
        {
            selection ??= new();

            if (elementData == null)
            {
                foreach (var selectedElement in selection.Values)
                    selectedElement.Deselect();

                selection.Clear();
            }
            else
            {
                selection.TryAdd(elementData.ID, elementData);
                elementData.Select();
            }
        }
    }
}
