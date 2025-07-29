using Shears.GraphViews.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMParameterBar : VisualElement
    {
        private readonly Dictionary<string, ParameterUI> parameterUIs = new();
        private readonly List<string> instanceParameterIDs = new();
        private readonly ContentSelector contentSelector;

        private StateMachineGraph graphData;
        private VisualElement titlePanel;
        private VisualElement contentPanel;
        private VisualElement parametersPanel;
        private VisualElement resizeBar;
        private VisualElement resizeBarVisual;
        private Button addButton;

        private readonly Color resizeDefaultColor = new(.251f, .251f, .251f);
        private readonly Color resizeHighlightColor = new(.4f, .4f, .4f);

        public IReadOnlyDictionary<string, ParameterUI> ParameterUIs => parameterUIs;

        public SMParameterBar(SMGraphView graphView, StateMachineGraph graphData)
        {
            contentSelector = new(graphView);

            AddToClassList(SMEditorUtil.ParameterBarClassName);
            this.AddStyleSheet(SMEditorUtil.ParameterBarStyleSheet);

            CreateTitlePanel();
            CreateContentPanel();

            SetGraphData(graphData);
        }

        #region Title Panel
        private void CreateTitlePanel()
        {
            titlePanel = new()
            {
                pickingMode = PickingMode.Ignore
            };

            titlePanel.AddToClassList(SMEditorUtil.ParameterBarTitlePanelClassName);

            CreateTitle();
            CreateAddButton();

            Add(titlePanel);
        }

        private void CreateTitle()
        {
            var title = new Label("Parameters");

            title.AddToClassList(SMEditorUtil.ParameterBarTitleClassName);

            titlePanel.Add(title);
        }

        private void CreateAddButton()
        {
            addButton = new(ShowContextMenu)
            {
                text = "+"
            };

            addButton.AddToClassList(SMEditorUtil.ParameterBarAddButtonClassName);

            if (graphData == null)
                addButton.enabledSelf = false;

            titlePanel.Add(addButton);
        }
        #endregion

        #region Content Panel
        private void CreateContentPanel()
        {
            contentPanel = new()
            {
                pickingMode = PickingMode.Ignore
            };

            contentPanel.AddToClassList(SMEditorUtil.ParameterBarContentPanelClassName);

            CreateParametersPanel();
            CreateResizeBar();

            Add(contentPanel);
        }

        private void CreateParametersPanel()
        {
            parametersPanel = new()
            {
                pickingMode = PickingMode.Ignore
            };

            parametersPanel.AddToClassList(SMEditorUtil.ParameterBarParametersPanelClassName);

            contentPanel.Add(parametersPanel);
        }
        #endregion

        #region Resize Bar
        private void CreateResizeBar()
        {
            resizeBar = new();
            resizeBarVisual = new()
            {
                pickingMode = PickingMode.Ignore,
            };

            resizeBar.AddToClassList(SMEditorUtil.ResizeBarClassName);
            resizeBarVisual.AddToClassList(SMEditorUtil.ResizeBarVisualClassName);

            resizeBar.AddManipulator(new Resizer(this, Resizer.ResizeDirection.Width));
            resizeBar.RegisterCallback<MouseOverEvent>(HighlightResizeBar);
            resizeBar.RegisterCallback<MouseOutEvent>(UnhighlightResizeBar);

            resizeBar.Add(resizeBarVisual);
            contentPanel.Add(resizeBar);
        }

        private void HighlightResizeBar(MouseOverEvent evt)
        {
            resizeBarVisual.style.backgroundColor = resizeHighlightColor;

            evt.StopImmediatePropagation();
        }

        private void UnhighlightResizeBar(MouseOutEvent evt)
        {
            resizeBarVisual.style.backgroundColor = resizeDefaultColor;
        }
        #endregion

        public void Reload()
        {
            instanceParameterIDs.Clear();
            instanceParameterIDs.AddRange(parameterUIs.Keys);

            foreach (var parameterID in instanceParameterIDs)
                RemoveParameterUI(parameterID);

            LoadParameters();
        }

        private void LoadParameters()
        {
            var parameters = new List<ParameterData>(graphData.GetParameters());

            foreach (var parameter in parameters)
                AddParameterUI(parameter, false);
        }

        public void SetGraphData(StateMachineGraph graphData)
        {
            if (graphData == null)
                return;

            if (this.graphData != null)
                ClearGraphData();

            this.graphData = graphData;

            addButton.enabledSelf = true;
            this.graphData.ParameterDataAdded += AddParameterUI;
            this.graphData.ParameterDataRemoved += RemoveParameterUI;

            this.AddManipulator(contentSelector);

            LoadParameters();
        }

        public void ClearGraphData()
        {
            if (graphData == null)
                return;

            graphData.ParameterDataAdded -= AddParameterUI;
            graphData.ParameterDataRemoved -= RemoveParameterUI;

            parameterUIs.Clear();

            addButton.enabledSelf = false;
            parametersPanel.Clear();

            this.RemoveManipulator(contentSelector);
        }
    
        private void ShowContextMenu()
        {
            GenericMenu menu = new();

            menu.AddItem(new GUIContent("Bool Parameter"), false, AddParameter, new BoolParameterData());
            menu.AddItem(new GUIContent("Int Parameter"), false, AddParameter, new IntParameterData());
            menu.AddItem(new GUIContent("Float Parameter"), false, AddParameter, new FloatParameterData());
            menu.AddItem(new GUIContent("Trigger Parameter"), false, AddParameter, new TriggerParameterData());

            menu.ShowAsContext();
        }

        private void AddParameter(object parameterObj)
        {
            if (parameterObj is not ParameterData parameter)
                throw new System.ArgumentException("Invalid argument for adding a parameter!");

            if (!graphData.IsUsableParameterName(parameter.Name))
            {
                var name = parameter.Name;
                int count = 0;
                while (!graphData.IsUsableParameterName(name))
                {
                    name = parameter.Name + " " + count;
                    ++count;
                }

                parameter.Name = name;
            }

            GraphViewEditorUtil.Record(graphData, "Add Parameter");
            graphData.AddParameter(parameter);
            GraphViewEditorUtil.Save(graphData);
        }

        private void AddParameterUI(ParameterData parameterData) => AddParameterUI(parameterData, true);
        private void AddParameterUI(ParameterData parameterData, bool renameByDefault)
        {
            var parameterUI = new ParameterUI(parameterData, graphData, Reload);

            parametersPanel.Add(parameterUI);
            parameterUIs.Add(parameterData.ID, parameterUI);

            if (renameByDefault)
                parameterUI.RenameParameter();
        }

        private void RemoveParameterUI(ParameterData parameterData) => RemoveParameterUI(parameterData.ID);
        private void RemoveParameterUI(string parameterID)
        {
            if (!parameterUIs.TryGetValue(parameterID, out var parameterUI))
                return;

            parameterUIs.Remove(parameterID);
            parametersPanel.Remove(parameterUI);
        }
    }
}
