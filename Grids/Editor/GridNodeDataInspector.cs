using System;
using Shears.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace Shears.Grids.Editor
{
    public class GridNodeDataInspector : VisualElement
    {
        private readonly SerializedProperty nodeDataProperty;
        private readonly Button contextButton;

        public SerializedProperty NodeDataProperty => nodeDataProperty;

        public event Action<SerializedProperty> RemoveRequested;
        public event Action<SerializedProperty> MoveUpRequested;
        public event Action<SerializedProperty> MoveDownRequested;

        public GridNodeDataInspector(SerializedProperty nodeDataProperty)
        {
            this.nodeDataProperty = nodeDataProperty;

            name = $"{nameof(GridNodeData)} Inspector";
            AddToClassList(ShearsGridStyles.NodeDataClass);

            var headerContainer = new VisualElement() { name = "Header Container" };
            headerContainer.AddToClassList(ShearsGridStyles.NodeDataHeaderContainerClass);

            string headerText = nodeDataProperty.managedReferenceValue.GetType().Name.PascalSpace();
            var header = new Label(headerText);
            header.AddToClassList(ShearsGridStyles.NodeDataHeaderClass);

            contextButton = new Button(OpenContextMenu)
            {
                name = "Context Button",
                iconImage = ShearsGridStyles.ContextMenuIcon,
            };
            contextButton.AddToClassList(ShearsGridStyles.NodeDataContextButtonClass);

            headerContainer.AddAll(header, contextButton);

            var contentContainer = new VisualElement() { name = "Content Container" };
            contentContainer.AddToClassList(ShearsGridStyles.NodeDataContentContainerClass);

            var defaultFields = VisualElementEditorUtil.CreateDefaultFields(nodeDataProperty);
            contentContainer.Add(defaultFields);

            this.AddAll(headerContainer, contentContainer);
        }

        private void OpenContextMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new("Remove"), false, () => RemoveRequested?.Invoke(nodeDataProperty));
            menu.AddItem(new("Move Up"), false, () => MoveUpRequested?.Invoke(nodeDataProperty));
            menu.AddItem(
                new("Move Down"),
                false,
                () => MoveDownRequested?.Invoke(nodeDataProperty)
            );

            menu.DropDown(contextButton.worldBound);
        }
    }
}
