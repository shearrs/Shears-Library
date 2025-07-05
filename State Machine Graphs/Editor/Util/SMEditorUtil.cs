using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public static class SMEditorUtil
    {
        private const string STYLE_SHEET_PATH = "State Machine Graph/Style Sheets";

        private static readonly StyleSheet graphStyleSheet = Resources.Load<StyleSheet>($"{STYLE_SHEET_PATH}/StateMachineGraph");
        private static readonly StyleSheet toolbarStyleSheet = Resources.Load<StyleSheet>($"{STYLE_SHEET_PATH}/StateMachineGraph");

        public static StyleSheet GraphStyleSheet => graphStyleSheet;
        public static StyleSheet ToolbarStyleSheet => toolbarStyleSheet;
    }
}
