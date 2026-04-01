using Shears.Logging;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Shears.StateMachineGraphs.Editor
{
    [InitializeOnLoad]
    public class StateMachineGraphCompiler : AssetModificationProcessor, IPreprocessBuildWithReport
    {
        private const bool LOGGING_ENABLED = false;

        public int callbackOrder => 0;
        
        static StateMachineGraphCompiler()
        {
        }

        public static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (var path in paths)
            {
                var graph = AssetDatabase.LoadAssetAtPath<StateMachineGraph>(path);

                if (graph == null)
                    continue;

                Compile(graph, path);
            }

            return paths;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            Log("Starting StateMachineGraph compilation...");
            CompileAllStateMachineGraphs();
        }

        [InitializeOnEnterPlayMode]
        public static void OnEnterPlayMode(EnterPlayModeOptions _)
        {
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

                if (graph == null)
                {
                    InternalLogError($"Failed to load StateMachineGraph at path: {path}");
                    continue;
                }

                if (!graph.NeedsCompilation)
                    continue;

                Compile(graph, path);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private static void Compile(StateMachineGraph graph, string path)
        {
            Log($"Compiling StateMachineGraph at path: {path}");

            var oldData = graph.GetData(true);

            if (oldData != null)
                AssetDatabase.RemoveObjectFromAsset(oldData);

            graph.Compile();
            var data = graph.GetData(true);

            AssetDatabase.AddObjectToAsset(data, path);
        }

        private static void Log(string message)
        {
#pragma warning disable CS0162 // Unreachable code detected
            if (LOGGING_ENABLED)
                SHLogger.Log(message);
#pragma warning restore CS0162 // Unreachable code detected
        }

        private static void InternalLogError(string message)
        {
#pragma warning disable CS0162 // Unreachable code detected
            if (LOGGING_ENABLED)
                SHLogger.Log(message, SHLogLevels.Error);
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}