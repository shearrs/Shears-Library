using UnityEditor;
using UnityEngine;

namespace Shears.GraphViews.Editor
{
    [FilePath("Shears Graph View", FilePathAttribute.Location.PreferencesFolder)]
    public class GraphEditorState : ScriptableSingleton<GraphEditorState>
    {
        [SerializeField] private GraphData graphData;

        public GraphData GraphData => graphData;

        public void SetGraphData(GraphData graphData)
        {
            this.graphData = graphData;
            Save(true);
        }
    }
}
