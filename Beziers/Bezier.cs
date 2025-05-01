using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Shears.Beziers
{
    [ExecuteInEditMode]
    public class Bezier : MonoBehaviour
    {
        private const float POINT_DISC_RADIUS = 0.2f;

        [SerializeField] private List<BezierPoint> points = new();

        private void OnValidate()
        {
            foreach (var point in points)
                point.Parent = transform;
        }

        private void Awake()
        {
            if (Application.isPlaying)
                return;
        }
    }
}
