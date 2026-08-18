using System;
using UnityEngine;

namespace Shears.Pathfinding
{
    [Serializable]
    public abstract class PathNodeData : ICloneable
    {
        public abstract Color EditorColor { get; }

        public event Action<PathNodeData> Updated;

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        public virtual void DrawHandles(PathNodeHandleContext context) { }

        protected void InvokeUpdated()
        {
            Updated?.Invoke(this);
        }
    }
}
