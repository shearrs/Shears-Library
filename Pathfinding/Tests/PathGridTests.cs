using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Shears.Pathfinding
{
    public class PathGridTests : MonoBehaviour
    {
        [SerializeField]
        private PathGridGroup group;

        [Header("Adding")]
        [SerializeField]
        private List<PathGrid> gridsToAdd;

        [Header("Shifting")]
        [SerializeField]
        private Direction direction;

        [SerializeField]
        private int distance;

        [SerializeField]
        private Vector3Int rangeStart;

        [SerializeField]
        private Vector3Int rangeEnd;

        [ContextMenu("Combine Grids")]
        private void Combine()
        {
            foreach (var grid in gridsToAdd)
                group.Add(grid);
        }

        [ContextMenu("Shift")]
        private void Shift()
        {
#if UNITY_EDITOR
            Undo.RecordObject(group.gameObject, "Shift");

            group.Shift(rangeStart, rangeEnd, direction, distance);

            EditorUtility.SetDirty(group);
#endif
        }
    }
}
