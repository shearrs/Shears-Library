using UnityEngine;

namespace Shears.Grids.Editor
{
    public static class ShearsGridStyles
    {
        private const string ResourcePath = "Shears Grid Styles";

        public const string EditorWindowStyleSheet = ResourcePath + "/GridEditorWindow";

        public const string RootContainerClass = "root-container";
        public const string ToolButtonContainerClass = "tool-button-container";
        public const string ToolButtonClass = "tool-button";
        public const string ContentContainerClass = "content-container";
        public const string ViewContainerClass = "view-container";
        public const string NodeInspectorClass = "node-inspector";
        public const string NodePainterClass = "node-painter";

        public const string NodeDataContainerClass = "node-data-container";
        public const string NodeDataClass = "node-data";
        public const string NodeDataCapClass = "node-data-cap";
        public const string NodeDataEndClass = "node-data-end";
        public const string NodeDataHeaderContainerClass = "node-data-header-container";
        public const string NodeDataHeaderClass = "node-data-header";
        public const string NodeDataContentContainerClass = "node-data-content-container";
        public const string NodeDataContextButtonClass = "node-data-context-button";
        public const string NodeDataAddButtonClass = "node-data-add-button";
        public const string NodeDataEmptyLabel = "node-data-empty-label";

        public static Texture2D SelectIcon =>
            Resources.Load<Texture2D>($"{ResourcePath}/SelectIcon");
        public static Texture2D PaintIcon => Resources.Load<Texture2D>($"{ResourcePath}/PaintIcon");
        public static Texture2D EraserIcon =>
            Resources.Load<Texture2D>($"{ResourcePath}/EraserIcon");
        public static Texture2D FillIcon => Resources.Load<Texture2D>($"{ResourcePath}/FillIcon");
        public static Texture2D ContextMenuIcon =>
            Resources.Load<Texture2D>($"{ResourcePath}/ContextMenuIcon");
    }
}
