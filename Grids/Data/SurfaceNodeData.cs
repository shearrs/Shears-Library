using System.Collections.Generic;
using UnityEngine;
using static Shears.Grids.SurfaceNodeData;

namespace Shears.Grids
{
    [System.Serializable]
    [DisallowMultipleComponent]
    public class SurfaceNodeData : PathNodeData
    {
        [SerializeField]
        private bool isSlope = false;

        [SerializeField, ShowIf(nameof(isSlope))]
        private SlopeDirection slopeDirection;

        protected override Color EditorColor => Color.yellowNice;
        public bool IsSlope => isSlope;
        public SlopeDirection SlopingDirection => slopeDirection;

        public enum SlopeDirection
        {
            UpLeft,
            UpRight,
            DownLeft,
            DownRight,
        }
    }

    public static class SlopeUtil
    {
        private static readonly Vector3 UP_LEFT_NORMAL = new Vector3(-1, 1, 0).normalized;
        private static readonly Vector3 UP_RIGHT_NORMAL = new Vector3(1, 1, 0).normalized;
        private static readonly Vector3 DOWN_LEFT_NORMAL = new Vector3(-1, -1, 0).normalized;
        private static readonly Vector3 DOWN_RIGHT_NORMAL = new Vector3(1, -1, 0).normalized;

        public static bool IsUpwardSlope(this SlopeDirection direction)
        {
            return direction == SlopeDirection.UpLeft || direction == SlopeDirection.UpRight;
        }

        public static Vector3 GetNormal(this SlopeDirection direction, ShearsGrid grid)
        {
            Vector3 normal = direction switch
            {
                SlopeDirection.UpLeft => UP_LEFT_NORMAL,
                SlopeDirection.UpRight => UP_RIGHT_NORMAL,
                SlopeDirection.DownLeft => DOWN_LEFT_NORMAL,
                SlopeDirection.DownRight => DOWN_RIGHT_NORMAL,
                _ => Vector3.zero,
            };

            return grid.transform.TransformDirection(normal);
        }
    }
}
