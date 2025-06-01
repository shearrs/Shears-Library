using UnityEngine;

namespace Shears.Detection
{
    public class Box2DDetector : AreaDetector2D
    {
        [Header("Box Settings")]
        [field: SerializeField] public Vector2 Center { get; set; }
        [field: SerializeField, Range(0, 360)] public float Angle { get; set; } = 0f;
        [field: SerializeField] public Vector2 Size { get; set; } = Vector2.one;

        protected override int Sweep(Collider2D[] detections)
        {
            var averageScale = (transform.lossyScale.x + transform.lossyScale.y) / 2f;
            var size = Size * averageScale;

            int hits = Physics2D.OverlapBox(transform.TransformPoint(Center), size, Angle, ContactFilter, detections);

            return hits;
        }

        protected override void DrawWireGizmos()
        {
            var originalMatrix = Gizmos.matrix;
            var averageScale = (transform.lossyScale.x + transform.lossyScale.y) / 2f;
            var size = Size * averageScale;

            Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(Center), Quaternion.Euler(0, 0, Angle) * transform.rotation, Vector3.one);

            Gizmos.DrawWireCube(Vector3.zero, size);

            Gizmos.matrix = originalMatrix;
        }
    }
}
