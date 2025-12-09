using Shears.Editor;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Pathfinding.Editor
{
    public class PGETUI : VisualElement
    {
        private readonly PGETSettings settings;
        private readonly Dictionary<int, PathNode> nodeHandles = new();
        private readonly Dictionary<Vector2Int, List<PathNode>> nodeHandleRows = new();
        private readonly List<MenuItem> menuItems = new();
        private readonly List<int> hoveredHandles = new();

        private VisualElement nodeDataContainer;
        private GenericMenu typeMenu;
        private Button typeButton;
        private SceneView sceneView;

        public event Action<Type> TypeSelected;

        private readonly struct MenuItem
        {
            private readonly string name;
            private readonly int order;
            private readonly Type type;

            public readonly string MenuPath => name;
            public readonly int Order => order;
            public readonly Type Type => type;

            public MenuItem(string name, int order, Type type)
            {
                this.name = name;
                this.order = order;
                this.type = type;
            }
        }

        public PGETUI(PGETSettings settings)
        {
            this.settings = settings;

            sceneView = EditorWindow.GetWindow<SceneView>();

            if (!sceneView.sceneViewState.fxEnabled)
                sceneView.sceneViewState.fxEnabled = true;
        }

        ~PGETUI()
        {
            
        }

        private void CreateUI()
        {
            var root = new VisualElement();

            root.style.marginTop = StyleKeyword.Auto;
            root.style.marginRight = StyleKeyword.Auto;
            root.style.marginLeft = 10;
            root.style.marginBottom = 10;
            root.SetAllPadding(4);
            root.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            root.SetAllBorderColors(new Color(0.1f, 0.1f, 0.1f, 0.8f));
            root.SetAllBorders(1);

            var depthSlider = new SliderInt("Z Depth", 0, settings.Grid.GridSize.z - 1)
            {
                showInputField = true
            };
            depthSlider.BindProperty(settings.ZDepthProp);
            depthSlider.style.marginBottom = 8;
            depthSlider.labelElement.style.minWidth = 80;
            depthSlider.hierarchy[1].hierarchy[0].style.minWidth = 60;

            var typeContainer = new VisualElement();
            typeContainer.style.flexDirection = FlexDirection.Row;

            var typeLabel = new Label("Data Type");

            string typeName = settings.NodeData == null ? "None" : settings.NodeData.GetType().Name;
            typeButton = new Button(typeMenu.ShowAsContext)
            {
                text = typeName
            };
            typeButton.style.marginLeft = 4;
            typeButton.style.flexGrow = 1;

            typeContainer.AddAll(typeLabel, typeButton);

            var prefabField = CreateNodePrefabField();

            var drawNodeDataField = new PropertyField(settings.DrawNodeDataProp);
            var drawPrefabField = new PropertyField(settings.DrawPrefabProp);
            var drawAllDepthsField = new PropertyField(settings.DrawAllDepthsProp);

            drawNodeDataField.BindProperty(settings.DrawNodeDataProp);
            drawPrefabField.BindProperty(settings.DrawPrefabProp);
            drawAllDepthsField.BindProperty(settings.DrawAllDepthsProp);

            nodeDataContainer = new VisualElement();
            nodeDataContainer.style.display = DisplayStyle.None;

            root.AddAll(nodeDataContainer, drawNodeDataField, drawPrefabField, drawAllDepthsField, depthSlider, prefabField, typeContainer);
            sceneView.rootVisualElement.Add(root);

            CreateNodeRows();

            if (settings.NodeData != null)
                UpdateNodeDataFields();
        }

        private void CreateTypeMenu()
        {
            typeMenu = new GenericMenu();

            typeMenu.AddItem(new("None"), false, () => OnTypeSelected(null));
            menuItems.Clear();

            foreach (var type in TypeCache.GetTypesDerivedFrom<PathNodeData>())
            {
                if (!type.IsAbstract)
                {
                    var attribute = type.GetCustomAttribute<NodeDataMenuItem>();
                    MenuItem menuItem;

                    if (attribute == null)
                        menuItem = new(type.Name, int.MaxValue, type);
                    else
                        menuItem = new(attribute.MenuPath, attribute.Order, type);

                    menuItems.Add(menuItem);
                }
            }

            menuItems.Sort((item1, item2) => item2.Order.CompareTo(item1.Order));

            foreach (var item in menuItems)
                typeMenu.AddItem(new(item.MenuPath), false, () => OnTypeSelected(item.Type));
        }

        private void OnTypeSelected(Type type)
        {
            TypeSelected?.Invoke(type);

            if (type != null)
                nodeDataProp.boxedValue = (PathNodeData)Activator.CreateInstance(type);
            else
                nodeDataProp.boxedValue = null;



            editorSO.ApplyModifiedProperties();
        }

        private void OnNodeDataChanged()
        {
            var type = settings.NodeData?.GetType();

            string typeName = type == null ? "None" : type.Name;
            typeButton.text = typeName;

            UpdateNodeDataFields();
        }
    }
}
