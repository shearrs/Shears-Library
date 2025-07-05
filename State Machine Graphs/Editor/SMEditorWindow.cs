using UnityEditor;
using UnityEngine;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMEditorWindow : EditorWindow
    {
        private SMGraphView graph;

        // need to save what data we have selected in a scriptablesingleton
        [MenuItem("Window/State Machine Graph")]
        public static void Open()
        {
            var window = GetWindow<SMEditorWindow>("State Machine Graph", typeof(SceneView));
            window.titleContent = new GUIContent("State Machine Graph");
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.name = "StateMachineGraphEditorWindow";
            root.AddToClassList("editorWindow");
            root.AddStyleSheet(SMEditorUtil.GraphStyleSheet);

            graph = new();

            root.Add(graph);
        }
    }
}
