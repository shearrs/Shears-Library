using UnityEngine;
using Shears;

namespace Shears.GraphViews
{
    [System.Serializable]
    internal class GraphSelectionDictionary : SerializableReferenceDictionary<string, GraphElementData> { }
    
    [System.Serializable]
    internal class GraphNodeDictionary : SerializableReferenceDictionary<string, GraphNodeData> { }
    
    [System.Serializable]
    internal class GraphEdgeDictionary : SerializableReferenceDictionary<string, GraphEdgeData> { }
}
