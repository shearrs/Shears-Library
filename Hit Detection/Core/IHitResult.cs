using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHitResult
    {
        public Vector3 Point { get; }
        public Vector3 Normal { get; }
        public float Distance { get; }
        public Transform Transform { get; }
    }
}
