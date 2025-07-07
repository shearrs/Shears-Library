using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public static class SMEditorUtil
    {
        private const string STYLE_SHEET_PATH = "State Machine Graph/Style Sheets";

        public static string ToolbarClassName => "toolBar";
        public static string ToolbarDataFieldClassName => "dataField";
        public static string ToolbarDataFieldLabelClassName => "dataFieldLabel";
        public static string LayerDisplayClassName => "layerDisplay";
        public static string LayerDisplayTagClassName => "layerDisplayTag";

        public static StyleSheet GraphStyleSheet => Resources.Load<StyleSheet>($"{STYLE_SHEET_PATH}/StateMachineGraph");
        public static StyleSheet ToolbarStyleSheet => Resources.Load<StyleSheet>($"{STYLE_SHEET_PATH}/StateMachineGraph");
    }
}
