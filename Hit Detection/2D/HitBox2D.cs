using UnityEngine;

namespace Shears.HitDetection
{
    public class HitBox2D : HitBody2D
    {
        [Header("Collision Settings")]
        [SerializeField] private Vector3 center;
        [SerializeField] private Quaternion orientation = Quaternion.identity;
        [SerializeField] private Vector2 halfExtents = new(0.5f, 0.5f);
        [SerializeField, Range(0.02f, 0.15f)] private float skinWidth = 0.025f;

        private readonly RaycastHit2D[] results = new RaycastHit2D[10];

        private Vector2 HalfExtents => Vector2.Scale(halfExtents, transform.lossyScale);
        private Quaternion Orientation => transform.localRotation * orientation;

        protected override void Sweep()
        {
            
        }

        private void BoxCastHits(Vector3 offset, Vector2 size, Vector2 direction, float distance)
        {
            var filter = new ContactFilter2D
            {
                useTriggers = true,
                layerMask = collisionMask,
                maxDepth = distance
            };

            var matrix = Matrix4x4.TRS(transform.position, Orientation, transform.lossyScale);

            int hits = Physics2D.BoxCast(matrix.MultiplyPoint3x4(center + offset), size, Orientation.eulerAngles.z, direction, filter, results);
        
            
        }

        private void AddValidHits(int hits)
        {
            for (int i = 0; i < hits; i++)
            {
                RaycastHit2D result = results[i];

                if (result.point != Vector2.zero && !finalHits.Contains(result))
                    finalHits.Add(result);
            }
        }
    }
}
