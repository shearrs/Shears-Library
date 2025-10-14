using System;
using UnityEngine;

namespace Shears.Pathfinding
{
    [Serializable]
    public abstract class PathNodeData : ICloneable
    {
        public abstract Color EditorColor { get; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public virtual void DrawGizmos(Vector3 nodePosition, float nodeSize) { }
    }
}
