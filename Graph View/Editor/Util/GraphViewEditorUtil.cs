using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public static class GraphViewEditorUtil
    {
        private static readonly string STYLE_SHEET_PATH = "Graph View/Style Sheets";

        public static string GraphViewClassName => "graphView";
        public static string RootContainerClassName => "rootContainer";
        public static string GraphViewContainerClassName => "graphViewContainer";
        public static string ContentViewContainerClassName => "contentViewContainer";
        public static string GridBackgroundClassName => "gridBackground";
        public static string GraphNodeClassName => "graphNode";

        public static StyleSheet GraphViewStyleSheet => Resources.Load<StyleSheet>($"{STYLE_SHEET_PATH}/GraphView");
    }
}
