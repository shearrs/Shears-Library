using System;
using System.Linq;
using UnityEngine;

namespace Shears.Grids
{
    public class ShiftTests : MonoBehaviour
    {
        [SerializeField]
        private ShearsGrid grid;

        [SerializeField]
        private Vector3Int min;

        [SerializeField]
        private Vector3Int max;

        [SerializeField]
        private Direction direction;

        [SerializeField]
        private int distance;

        [SerializeField]
        private Vector3Int[] ignorePositions = Array.Empty<Vector3Int>();

        [ContextMenu("Shift")]
        private void Shift()
        {
            grid.Shift(min, max, direction, distance, ignorePositions.ToHashSet());
        }
    }
}
