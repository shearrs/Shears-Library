using System;
using UnityEngine;

namespace Shears.Grids
{
    [Serializable]
    public abstract class GridNodeData
    {
        public static readonly Color DEFAULT_EDITOR_COLOR = new(1.0f, 1.0f, 1.0f, 0.5f);

        internal Color InternalEditorColor => EditorColor;
        internal bool InternalEditorAutomaticTarget => EditorAutomaticTarget;
        protected virtual Color EditorColor { get; } = DEFAULT_EDITOR_COLOR;
        protected virtual bool EditorAutomaticTarget { get; } = true;

        internal event Action<GridNodeData> Updated;

        internal void Dispose() { }

        protected void BroadcastUpdate()
        {
            Updated?.Invoke(this);
        }

        internal void DrawHandles(GridNodeHandleContext context)
        {
            OnDrawHandles(context);
        }

        protected virtual void OnDrawHandles(GridNodeHandleContext context) { }
    }
}
