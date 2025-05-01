using UnityEngine;

namespace Shears.Beziers
{
    [System.Serializable]
    public class BezierPoint
    {
        [field: SerializeField] public Vector3 Position { get; set; } = Vector3.zero;
        [field: SerializeField] public Quaternion Rotation { get; set; } = Quaternion.identity;
        [field: SerializeField] public Vector3 Scale { get; set; } = Vector3.one;
        [field: SerializeField] public Vector3 ControlPoint1 { get; set; }
        [field: SerializeField] public Vector3 ControlPoint2 { get; set; }

        public Transform Parent { get; set; }
    }
}
