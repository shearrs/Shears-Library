using Shears.Logging;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Shears.StateMachineGraphs.Editor
{
    [InitializeOnLoad]
    public class StateMachineGraphCompiler : IPreprocessBuildWithReport
    {
        private const bool LOGGING_ENABLED = false;

        public int callbackOrder => 0;

        static StateMachineGraphCompiler()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            Log("Starting StateMachineGraph compilation...");
            CompileAllStateMachineGraphs();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange stateChange)
        {
            if (stateChange != PlayModeStateChange.ExitingEditMode)
                return;

            Log("Entering Play Mode: Compiling StateMachineGraphs...");
            CompileAllStateMachineGraphs();
        }

        private static void CompileAllStateMachineGraphs()
        {
            var guids = AssetDatabase.FindAssets("t:StateMachineGraph");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var graph = AssetDatabase.LoadAssetAtPath<StateMachineGraph>(path);

                Log($"Compiling StateMachineGraph at path: {path}");

                if (graph == null)
                {
                    LogError($"Failed to load StateMachineGraph at path: {path}");
                    continue;
                }

                graph.Compile();
                EditorUtility.SetDirty(graph);
            }
        }

        private static void Log(string message)
        {
            if (LOGGING_ENABLED)
#pragma warning disable CS0162 // Unreachable code detected
                SHLogger.Log(message);
#pragma warning restore CS0162 // Unreachable code detected
        }

        private static void LogError(string message)
        {
            if (LOGGING_ENABLED)
#pragma warning disable CS0162 // Unreachable code detected
                SHLogger.Log(message, SHLogLevels.Error);
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}