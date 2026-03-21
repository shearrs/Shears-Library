using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public static class SMEditorUtil
    {
        private const string STYLE_SHEET_PATH = "State Machine Graph/Style Sheets";

        #region Node Class Names
        public static readonly string LayerDefaultNodeClassName = "layerDefault";
        public static readonly string ExternalStateMachineNodeClassName = "externalStateMachineNode";
        public static readonly string EmptyTagClassName = "emptyTag";
        #endregion

        #region Toolbar Class Names
        public static readonly string ToolbarClassName = "toolBar";
        public static readonly string ToolbarDataFieldClassName = "dataField";
        public static readonly string ToolbarDataFieldLabelClassName = "dataFieldLabel";
        #endregion

        #region Layer Class Names
        public static readonly string LayerDisplayClassName = "layerDisplay";
        public static readonly string LayerDisplayTagClassName = "layerDisplayTag";
        #endregion

        #region Parameter Bar Class Names
        public static readonly string ParameterBarClassName = "parameterBar";
        public static readonly string ParameterBarTitlePanelClassName = "titlePanel";
        public static readonly string ParameterBarTitleClassName = "title";
        public static readonly string ParameterBarAddButtonClassName = "addButton";
        public static readonly string ResizeBarClassName = "resizeBar";
        public static readonly string ResizeBarVisualClassName = "resizeBarVisual";
        public static readonly string ParameterBarContentPanelClassName = "contentPanel";
        public static readonly string ParameterBarParametersPanelClassName = "parametersPanel";
        public static readonly string ParameterBarScrollViewClassName = "parametersScrollView";
        #endregion

        #region ParameterUI Class Names
        public static readonly string ParameterUIClassName = "parameterUI";
        public static readonly string ParameterUIToggleClassName = "toggle";
        public static readonly string ParameterUIIntFieldClassName = "intField";
        public static readonly string ParameterUISelectedClassName = "parameterUISelected";
        public static readonly string ParameterUIMovementButtonsClassName = "movementButtons";
        public static readonly string EditableLabelClassName = "editableLabel";
        public static readonly string EditableLabelLabelClassName = "elLabel";
        public static readonly string EditableLabelTextFieldClassName = "elTextField";
        #endregion

        #region Inspector Class Names
        public static readonly string SMGraphInspectorClassName = "graphInspector";
        public static readonly string StateNodeInspectorClassName = "stateNodeInspector";
        public static readonly string ParameterInspectorClassName = "parameterInspector";
        public static readonly string NodeTitleInspectorClassName = "nodeTitle";
        public static readonly string StateSelectorClassName = "stateSelector";

        #region Transition Class Names
        public static readonly string TransitionContainerClassName = "transitionContainer";
        public static readonly string TransitionContainerTitleClassName = "transitionContainerTitle";
        public static readonly string TransitionClassName = "transition";
        public static readonly string TransitionTitleClassName = "transitionTitle";
        #endregion

        #region Comparison Class Names
        public static readonly string TransitionsContainerClassName = "transitionsContainer";
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
