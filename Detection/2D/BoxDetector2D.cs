using UnityEngine;

namespace Shears.Detection
{
    public class BoxDetector2D : AreaDetector2D
    {
        [Header("Box Settings")]
        [SerializeField] private Vector2 offset = Vector2.zero;
        [SerializeField, Range(0, 360)] private float angle = 0;
        [SerializeField] private Vector2 size = Vector2.one;

        public Vector2 Offset { get => offset; set => offset = value; }
        public float Angle { get => angle; set => angle = value; }
        public Vector2 Size { get => size; set => size = value; }

        protected override int Sweep(Collider2D[] detections)
        {
            var averageScale = (transform.lossyScale.x + transform.lossyScale.y) / 2f;
            var size = Size * averageScale;

            int hits = Physics2D.OverlapBox(transform.TransformPoint(Offset), size, Angle, ContactFilter, detections);

            return hits;
        }

        protected override void DrawWireGizmos()
        {
            var originalMatrix = Gizmos.matrix;
            var averageScale = (transform.lossyScale.x + transform.lossyScale.y) / 2f;
            var size = Size * averageScale;

            Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(Offset), Quaternion.Euler(0, 0, Angle) * transform.rotation, Vector3.one);

            Gizmos.DrawWireCube(Vector3.zero, size);

            Gizmos.matrix = originalMatrix;
        }
    }
}
