using UnityEngine;

namespace Shears.Grids
{
    public class SurfaceEntityPosition : EntityPosition
    {
        private readonly SurfaceNodeData surfaceData;

        public Vector3Int SurfaceGridPosition { get; }
        public Vector3 SurfaceNormal { get; }
        public Direction SurfaceDirection { get; }
        public bool IsWalkable => surfaceData.IsWalkable;
        public bool IsSlope => surfaceData.IsSlope;
        public SurfaceNodeData.SlopeDirection SlopeDirection => surfaceData.SlopingDirection;

        public SurfaceEntityPosition(
            GridNode node,
            Vector3Int gridPosition,
            Vector3 worldPosition,
            Vector3Int surfaceGridPosition,
            Vector3 surfaceNormal,
            Direction surfaceDirection,
            SurfaceNodeData surfaceData
        )
            : base(node, gridPosition, worldPosition)
        {
            SurfaceGridPosition = surfaceGridPosition;
            SurfaceNormal = surfaceNormal;
            SurfaceDirection = surfaceDirection;
            this.surfaceData = surfaceData;
        }
    }
}
