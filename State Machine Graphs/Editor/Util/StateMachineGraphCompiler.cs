using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Shears.StateMachineGraphs.Editor
{
    [InitializeOnLoad]
    public class StateMachineGraphCompiler : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        static StateMachineGraphCompiler()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("Starting StateMachineGraph compilation...");
            CompileAllStateMachineGraphs();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange stateChange)
        {
            if (stateChange != PlayModeStateChange.ExitingEditMode) 
                return;

            Debug.Log("Entering Play Mode: Compiling StateMachineGraphs...");
            CompileAllStateMachineGraphs();
        }

        private static void CompileAllStateMachineGraphs()
        {
            var guids = AssetDatabase.FindAssets("t:StateMachineGraph");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var graph = AssetDatabase.LoadAssetAtPath<StateMachineGraph>(path);

                Debug.Log($"Compiling StateMachineGraph at path: {path}");

                if (graph == null)
                {
                    Debug.LogError($"Failed to load StateMachineGraph at path: {path}");
                    continue;
                }

                graph.Compile();
                EditorUtility.SetDirty(graph);
            }
        }
    }
}