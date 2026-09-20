using System;
using Shears.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Grids.Editor
{
    public class ShearsGridEditorWindow : EditorWindow
    {
        private Button currentButton;
        private Button selectButton;
        private Button paintButton;
        private Button eraserButton;
        private Button fillButton;
        private VisualElement contentContainer;
        private VisualElement viewContainer;
        private VisualElement toolContextContainer;
        private VisualElement inspectorContainer;
        private VisualElement paintContainer;
        private VisualElement fillContainer;

        private ShearsGridToolState State => ShearsGridToolState.instance;

        public static ShearsGridEditorWindow Open()
        {
            var window = GetWindow<ShearsGridEditorWindow>();
            window.titleContent = new("Shears Grid Editor");

            return window;
        }

        [MenuItem("Shears Library/Grid Editor")]
        private static void ManualOpen()
        {
            Open();
        }

        public void InitializeState()
        {
            State.CurrentTool.Bind(OnToolSelected);
            InitializeContainers();
            OnToolSelected(State.CurrentTool.Value);
        }

        public void ClearState()
        {
            State.CurrentTool.Unbind(OnToolSelected);
            ClearContainers();
            ClearToolButton();
        }

        private void OnDestroy()
        {
            State.CurrentTool.Unbind(OnToolSelected);
        }

        public void CreateGUI()
        {
            var root = new VisualElement() { name = "Grid Editor" };
            root.AddStyleSheet(ShearsGridStyles.EditorWindowStyleSheet);
            root.AddToClassList(ShearsGridStyles.RootContainerClass);

            contentContainer = CreateContentContainer();
            var buttonContainer = CreateButtonContainer();
            viewContainer = CreateViewContainer();
            toolContextContainer = CreateToolContextContainer();
            inspectorContainer = CreateInspectorContainer();
            paintContainer = CreatePaintContainer();
            fillContainer = CreateFillContainer();

            toolContextContainer.Add(inspectorContainer);
            contentContainer.AddAll(viewContainer, toolContextContainer);
            root.AddAll(buttonContainer, contentContainer);
            rootVisualElement.Add(root);
        }

        public void InspectNode(GridNode node)
        {
            inspectorContainer.Clear();

            if (node == null)
                return;

            var nodeProp = State.GetNodeProperty(node.GridPosition);
            int nodeIndex = State.Grid.GetNodeIndex(node.GridPosition);
            var inspector = new GridNodeInspector(nodeProp, nodeIndex);

            inspectorContainer.Add(inspector);
        }

        private VisualElement CreateContentContainer()
        {
            var contentContainer = new VisualElement() { name = "Content Container" };
            contentContainer.AddToClassList(ShearsGridStyles.ContentContainerClass);

            return contentContainer;
        }

        private VisualElement CreateButtonContainer()
        {
            var container = new VisualElement() { name = "Tool Button Container" };
            container.AddToClassList(ShearsGridStyles.ToolButtonContainerClass);

            selectButton = CreateButton(
                "Select Tool Button",
                "Select Tool",
                ShearsGridStyles.SelectIcon,
                () => SetTool(GridTool.Select)
            );
            selectButton.style.borderRightWidth = 0;
            selectButton.style.borderTopRightRadius = 0;
            selectButton.style.borderBottomRightRadius = 0;

            paintButton = CreateButton(
                "Paint Tool Button",
                "Paint Tool",
                ShearsGridStyles.PaintIcon,
                () => SetTool(GridTool.Paint)
            );
            paintButton.style.borderRightWidth = 0;
            paintButton.SetAllBorderRadii(0);

            eraserButton = CreateButton(
                "Eraser Tool Button",
                "Eraser Tool",
                ShearsGridStyles.EraserIcon,
                () => SetTool(GridTool.Eraser)
            );
            eraserButton.style.borderRightWidth = 0;
            eraserButton.SetAllBorderRadii(0);

            fillButton = CreateButton(
                "Fill Tool Button",
                "Fill Tool",
                ShearsGridStyles.FillIcon,
                () => SetTool(GridTool.Fill)
            );
            fillButton.style.borderTopLeftRadius = 0;
            fillButton.style.borderBottomLeftRadius = 0;

            container.AddAll(selectButton, paintButton, eraserButton, fillButton);

            return container;
        }

        private VisualElement CreateViewContainer()
        {
            var viewContainer = new VisualElement() { name = "View Container" };
            viewContainer.AddToClassList(ShearsGridStyles.ViewContainerClass);

            return viewContainer;
        }

        private VisualElement CreateToolContextContainer()
        {
            var toolContextContainer = new VisualElement() { name = "Tool Context Container" };

            return toolContextContainer;
        }

        private VisualElement CreateInspectorContainer()
        {
            var inspectorContainer = new VisualElement() { name = "Inspector Container" };

            return inspectorContainer;
        }

        private VisualElement CreatePaintContainer()
        {
            var paintContainer = new VisualElement() { name = "Paint Container" };
            paintContainer.AddToClassList(ShearsGridStyles.NodePainterClass);

            return paintContainer;
        }

        private VisualElement CreateFillContainer()
        {
            var fillContainer = new VisualElement() { name = "Fill Container" };
            fillContainer.AddToClassList(ShearsGridStyles.NodePainterClass);

            return fillContainer;
        }

        private void InitializeContainers()
        {
            InitializeViewContainer();
            InitializePaintContainer();
            InitializeFillContainer();
        }

        private void ClearContainers()
        {
            viewContainer.Clear();
            toolContextContainer.Clear();
            inspectorContainer.Clear();
            paintContainer.Clear();
            fillContainer.Clear();
        }

        private void InitializeViewContainer()
        {
            var header = VisualElementEditorUtil.CreateHeader("View Settings");
            var settingsContainer = new VisualElement() { name = "Settings Container" };

            var viewModeField = new PropertyField(State.ViewModeProperty);
            var axisField = new PropertyField(State.ViewAxisProperty);

            int getMaxDepth()
            {
                var gridSize = State.GridSizeProperty.vector3IntValue;
                var viewAxis = (ShearsGridToolState.Axis)State.ViewAxisProperty.enumValueIndex;

                return viewAxis switch
                {
                    ShearsGridToolState.Axis.X => gridSize.x - 1,
                    ShearsGridToolState.Axis.Y => gridSize.y - 1,
                    ShearsGridToolState.Axis.Z => gridSize.z - 1,
                    _ => 1,
                };
            }

            var depthField = new SliderInt(0, getMaxDepth())
            {
                label = State.ViewDepthProperty.displayName,
                showInputField = true,
            };

            if (State.ViewingMode == ShearsGridToolState.ViewMode.Automatic)
                depthField.style.display = DisplayStyle.None;

            viewModeField.BindProperty(State.ViewModeProperty);
            axisField.BindProperty(State.ViewAxisProperty);
            depthField.BindProperty(State.ViewDepthProperty);

            viewModeField.RegisterValueChangeCallback(evt =>
            {
                var value = (ShearsGridToolState.ViewMode)evt.changedProperty.enumValueIndex;
                if (value == ShearsGridToolState.ViewMode.Automatic)
                    depthField.style.display = DisplayStyle.None;
                else
                    depthField.style.display = DisplayStyle.Flex;
            });
            depthField.TrackPropertyValue(
                State.GridSizeProperty,
                p => depthField.highValue = getMaxDepth()
            );
            axisField.TrackPropertyValue(
                State.ViewAxisProperty,
                _ => depthField.highValue = getMaxDepth()
            );

            settingsContainer.AddAll(viewModeField, axisField, depthField);
            viewContainer.AddAll(header, settingsContainer);
        }

        private void InitializePaintContainer()
        {
            var header = VisualElementEditorUtil.CreateHeader("Paint Settings");

            var definitionField = new PropertyField();
            var brushSizeField = new PropertyField(State.BrushSizeProperty);

            definitionField.BindProperty(State.SelectedDefinitionProperty);
            brushSizeField.BindProperty(State.BrushSizeProperty);

            brushSizeField.RegisterValueChangeCallback(evt =>
            {
                var prop = evt.changedProperty;
                var value = prop.vector3IntValue;
                bool changed = false;

                if (value.x <= 0)
                {
                    value.x = 1;
                    changed = true;
                }

                if (value.y <= 0)
                {
                    value.y = 1;
                    changed = true;
                }

                if (value.z <= 0)
                {
                    value.z = 1;
                    changed = true;
                }

                if (changed)
                {
                    prop.vector3IntValue = value;
                    prop.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            });

            paintContainer.AddAll(header, definitionField, brushSizeField);
        }

        private void InitializeFillContainer()
        {
            var header = VisualElementEditorUtil.CreateHeader("Fill Settings");

            var definitionField = new PropertyField();
            definitionField.BindProperty(State.SelectedFillDefinitionProperty);

            fillContainer.AddAll(header, definitionField);
        }

        private Button CreateButton(string name, string tooltip, Background icon, Action callback)
        {
            var button = new Button(callback)
            {
                name = name,
                tooltip = tooltip,
                iconImage = icon,
            };

            button.AddToClassList(ShearsGridStyles.ToolButtonClass);
            button.AddToClassList("unity-toolbar-button");

            return button;
        }

        private void SetTool(GridTool tool)
        {
            if (State == null)
                return;

            State.SetTool(tool);
        }

        private void OnToolSelected(GridTool tool)
        {
            SelectToolButton(tool);
            UpdateContextContainer(tool);
        }

        private void UpdateContextContainer(GridTool tool)
        {
            toolContextContainer.Clear();

            switch (tool)
            {
                case GridTool.Select:
                    toolContextContainer.Add(inspectorContainer);
                    InspectNode(State.SelectedNode);
                    break;
                case GridTool.Paint:
                    toolContextContainer.Add(paintContainer);
                    break;
                case GridTool.Fill:
                    toolContextContainer.Add(fillContainer);
                    break;
            }
        }

        private void SelectToolButton(GridTool tool)
        {
            var newButton = tool switch
            {
                GridTool.Select => selectButton,
                GridTool.Paint => paintButton,
                GridTool.Eraser => eraserButton,
                GridTool.Fill => fillButton,
                _ => null,
            };

            if (currentButton == newButton)
                return;

            currentButton?.SetCheckedPseudoState(false);

            currentButton = newButton;

            currentButton?.SetCheckedPseudoState(true);
        }

        private void ClearToolButton()
        {
            currentButton?.SetCheckedPseudoState(false);
            currentButton = null;
        }
    }
}
