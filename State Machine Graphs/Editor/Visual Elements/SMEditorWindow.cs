using Shears.GraphViews.Editor;
using UnityEditor;
using UnityEngine;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMEditorWindow : GraphViewEditorWindow
    {
        private static StateMachineGraph selectedGraph;
        private static SMGraphView graph;

        [UnityEditor.Callbacks.OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int _)
        {
            var targetObject = EditorUtility.EntityIdToObject(instanceID);
            var graph = targetObject as StateMachineGraph;

            if (graph == null)
                return false;

            selectedGraph = graph;
            Open();
            return true;
        }

        [MenuItem("Shears Library/State Machine Graph")]
        public static void Open()
        {
            var window = GetWindow<SMEditorWindow>("State Machine Graph", typeof(SceneView));
            window.titleContent = new GUIContent("State Machine Graph");

            graph?.SetGraphData(selectedGraph);
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.name = "StateMachineGraphEditorWindow";

            graph = new();
            
            if (selectedGraph != null)
                graph.SetGraphData(selectedGraph);

            root.Add(graph);
        }
    }
}
