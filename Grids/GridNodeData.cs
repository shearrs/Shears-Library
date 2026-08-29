using UnityEngine;

namespace Shears.Grids
{
    [System.Serializable]
    public abstract class GridNodeData
    {
        public static readonly Color DEFAULT_EDITOR_COLOR = new(1.0f, 1.0f, 1.0f, 0.5f);

        internal Color InternalEditorColor => EditorColor;
        internal bool InternalEditorAutomaticTarget => EditorAutomaticTarget;
        protected virtual Color EditorColor { get; } = DEFAULT_EDITOR_COLOR;
        protected virtual bool EditorAutomaticTarget { get; } = true;

        internal void Dispose() { }

        protected virtual void OnDetached() { }
    }
}
