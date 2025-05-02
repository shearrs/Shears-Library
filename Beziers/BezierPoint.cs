using UnityEngine;

namespace Shears.Beziers
{
    [System.Serializable]
    public class BezierPoint
    {
        [SerializeField] private Vector3 position = Vector3.zero;
        [SerializeField] private Vector3 rotation = Vector3.zero;
        [SerializeField] private Vector3 tangent = Vector3.zero;

        public Vector3 LocalPosition { get => position; set => position = value; }
        public Quaternion LocalRotation { get => Quaternion.Euler(rotation); set => rotation = value.eulerAngles; }
        public Vector3 LocalTangent1 { get => tangent; set => tangent = value; }
        public Vector3 LocalTangent2 { get => -tangent; set => tangent = -value; }
        public Vector3 Position { get => Parent.TransformPoint(position); set => position = Parent.InverseTransformPoint(value); }
        public Quaternion Rotation { get => Parent.rotation * Quaternion.Euler(rotation); set => rotation = (Quaternion.Inverse(Parent.rotation) * value).eulerAngles; }
        public Vector3 Tangent1 { get => TransformPoint(tangent); set => tangent = InverseTransformPoint(value); }
        public Vector3 Tangent2 { get => TransformPoint(-tangent); set => tangent = -InverseTransformPoint(value); }

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
