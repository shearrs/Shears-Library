using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.GraphViews
{
    [Serializable]
    public class GraphNodeClipboardData : GraphElementClipboardData
    {
        [SerializeField] private string name;
        [SerializeField] private Vector2 position;
        [SerializeField] private List<string> edges = new();
        [SerializeField] private string parentID = GraphLayer.ROOT_ID;

        public string Name => name;
        public Vector2 Position => position;
        public IReadOnlyList<string> Edges => edges;
        public string ParentID => parentID;

        public GraphNodeClipboardData(string name, Vector2 position, List<string> edges, string parentID)
        {
            this.name = name;
            this.position = position;
            this.edges = edges ?? new List<string>();
            this.parentID = parentID;
        }
    }

    [Serializable]
    public class StateNodeClipboardData : GraphNodeClipboardData
    {
        [SerializeField] private SerializableSystemType stateType;

        public StateNodeClipboardData(string name, Vector2 position, List<string> edges, string parentID, SerializableSystemType stateType) : base(name, position, edges, parentID)
        {
            this.stateType = stateType;
        }
    }
}