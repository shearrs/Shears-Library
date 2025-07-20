using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public static class SMEditorUtil
    {
        private const string STYLE_SHEET_PATH = "State Machine Graph/Style Sheets";

        #region Node Class Names
        public static readonly string ExternalStateMachineNodeClassName = "externalStateMachineNode";
        #endregion

        #region Toolbar Class Names
        public static string ToolbarClassName => "toolBar";
        public static string ToolbarDataFieldClassName => "dataField";
        public static string ToolbarDataFieldLabelClassName => "dataFieldLabel";
        #endregion

        #region Layer Class Names
        public static string LayerDisplayClassName => "layerDisplay";
        public static string LayerDisplayTagClassName => "layerDisplayTag";
        #endregion

        #region Parameter Bar Class Names
        public static string ParameterBarClassName => "parameterBar";
        public static string ParameterBarTitlePanelClassName => "titlePanel";
        public static string ParameterBarTitleClassName => "title";
        public static string ParameterBarAddButtonClassName => "addButton";
        public static string ResizeBarClassName => "resizeBar";
        public static string ResizeBarVisualClassName => "resizeBarVisual";
        public static string ParameterBarContentPanelClassName => "contentPanel";
        public static string ParameterBarParametersPanelClassName => "parametersPanel";
        #endregion

        #region ParameterUI Class Names
        public static string ParameterUIClassName => "parameterUI";
        public static string ParameterUIToggleClassName => "toggle";
        public static string ParameterUIIntFieldClassName => "intField";
        public static string ParameterUISelectedClassName => "parameterUISelected";
        public static string EditableLabelClassName => "editableLabel";
        public static string EditableLabelLabelClassName => "elLabel";
        public static string EditableLabelTextFieldClassName => "elTextField";
        #endregion

        #region Inspector Class Names
        public static readonly string SMGraphInspectorClassName = "graphInspector";
        public static readonly string StateNodeInspectorClassName = "stateNodeInspector";
        public static readonly string NodeTitleInspectorClassName = "nodeTitle";
        public static readonly string StateSelectorClassName = "stateSelector";

        #region Transition Class Names
        public static readonly string TransitionContainerClassName = "transitionContainer";
        public static readonly string TransitionContainerTitleClassName = "transitionContainerTitle";
        public static readonly string TransitionClassName = "transition";
        public static readonly string TransitionTitleClassName = "transitionTitle";
        #endregion

        #region Comparison Class Names
        public static readonly string ComparisonsContainerClassName = "comparisonsContainer";
        public static readonly string AddComparisonButtonClassName = "addComparisonButton";
        public static readonly string RemoveComparisonButtonClassName = "removeComparisonButton";
        public static readonly string ComparisonBodyClassName = "comparisonBody";
        public static readonly string ComparisonDropdownClassName = "comparisonDropdown";
        public static readonly string ComparisonToggleClassName = "comparisonToggle";
        public static readonly string ComparisonLabelClassName = "comparisonLabel";
        public static readonly string ComparisonIntFieldClassName = "comparisonIntField";
        public static readonly string CompareTypeDropdownClassName = "compareTypeDropdown";
        #endregion
        #endregion

        public static StyleSheet GraphStyleSheet => Resources.Load<StyleSheet>($"{STYLE_SHEET_PATH}/StateMachineGraph");
        public static StyleSheet ToolbarStyleSheet => Resources.Load<StyleSheet>($"{STYLE_SHEET_PATH}/StateMachineGraph");
        public static StyleSheet ParameterBarStyleSheet => Resources.Load<StyleSheet>($"{STYLE_SHEET_PATH}/ParameterBar");
        public static StyleSheet SMGraphInspectorStyleSheet => Resources.Load<StyleSheet>($"{STYLE_SHEET_PATH}/StateMachineInspector");
    }
}
