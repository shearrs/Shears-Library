using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public static class GraphViewEditorUtil
    {
        private static readonly string STYLE_SHEET_PATH = "Graph View/Style Sheets";

        public static StyleSheet GraphViewStyleSheet => Resources.Load<StyleSheet>($"{STYLE_SHEET_PATH}/GraphView");
    }
}
