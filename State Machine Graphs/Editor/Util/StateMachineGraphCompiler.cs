using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Shears.StateMachineGraphs.Editor
{
    public class StateMachineGraphCompiler : IPreprocessBuildWithReport
    {
        int IOrderedCallback.callbackOrder => 100;

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            // find every StateMachineGraph in the project and call Compile on it
            var guids = AssetDatabase.FindAssets("t:StateMachineGraph");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var graph = AssetDatabase.LoadAssetAtPath<StateMachineGraph>(path);

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