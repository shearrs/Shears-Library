using Shears.GraphViews.Editor;
using UnityEditor;
using UnityEngine;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMEditorWindow : GraphViewEditorWindow
    {
        private static SMGraphView graph;

        public static SMGraphView GraphView => graph;

        [UnityEditor.Callbacks.OnOpenAsset]
        public static bool OnOpenAsset(EntityId entityID, int _)
        {
            var targetObject = EditorUtility.EntityIdToObject(entityID);
            var graph = targetObject as StateMachineGraph;

            if (graph == null)
                return false;

            GraphEditorState.instance.SetGraphData(graph);
            Open();
            return true;
        }

        [MenuItem("Shears Library/State Machine Graph")]
        public static void Open()
        {
            var window = GetWindow<SMEditorWindow>("State Machine Graph", typeof(SceneView));
            window.titleContent = new GUIContent("State Machine Graph");

            graph?.SetGraphData(GraphEditorState.instance.GraphData);
        }

        public void CreateGUI()
        {
            CreateView();
        }

        public void CreateView()
        {
            var window = GetWindow<SMEditorWindow>("State Machine Graph", typeof(SceneView));
            var root = window.rootVisualElement;
            root.name = "State Machine Graph Editor Window";
            root.Clear();

            graph = new();

            root.Add(graph);
        }
    }
}
