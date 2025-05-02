using UnityEngine;

namespace Shears.Beziers
{
    [System.Serializable]
    public class BezierPoint
    {
        [SerializeField] private Vector3 position = Vector3.zero;
        [SerializeField] private Vector3 rotation = Vector3.zero;
        [SerializeField] private Vector3 tangent = Vector3.zero;

        public Vector3 Position { get => Parent.TransformPoint(position); set => position = value; }
        public Quaternion Rotation { get => Parent.rotation * Quaternion.Euler(rotation); set => rotation = value.eulerAngles; }
        public Vector3 Tangent1 { get => Parent.TransformPoint(Quaternion.Euler(rotation) * tangent + position); set => tangent = value; }
        public Vector3 Tangent2 { get => Parent.TransformPoint(Quaternion.Euler(rotation) * -tangent + position); set => tangent = -value; }

        public Transform Parent { get; set; }

        public Vector3 TransformPoint(Vector3 point)
        {
            return Parent.TransformPoint(Quaternion.Euler(rotation) * point + position);
        }

        public Vector3 InverseTransformPoint(Vector3 point)
        {
            return Quaternion.Inverse(Quaternion.Euler(rotation)) * (Parent.InverseTransformPoint(point) - position);
        }
    }
}
