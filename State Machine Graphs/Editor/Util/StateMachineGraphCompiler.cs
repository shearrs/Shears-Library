using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Shears.StateMachineGraphs.Editor
{
    public class StateMachineGraphCompiler : BuildPlayerProcessor
    {
        public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
        {
            Debug.Log("Preparing for build: Compiling StateMachineGraphs...");
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("Starting StateMachineGraph compilation...");

            //var guids = AssetDatabase.FindAssets("t:StateMachineGraph");

            //foreach (var guid in guids)
            //{
            //    var path = AssetDatabase.GUIDToAssetPath(guid);
            //    var graph = AssetDatabase.LoadAssetAtPath<StateMachineGraph>(path);

            //    Debug.Log($"Compiling StateMachineGraph at path: {path}");

            //    if (graph == null)
            //    {
            //        Debug.LogError($"Failed to load StateMachineGraph at path: {path}");
            //        continue;
            //    }

            //    graph.Compile();
            //    EditorUtility.SetDirty(graph);
            //}
        }
    }
}