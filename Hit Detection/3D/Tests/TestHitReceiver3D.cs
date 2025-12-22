using UnityEngine;

namespace Shears.HitDetection
{
    public class TestHitReceiver3D : MonoBehaviour
    {
        private static readonly Color NOT_HIT_COLOR = Color.green;
        private static readonly Color HIT_COLOR = Color.red;

        [SerializeField] private HurtBody3D body;
        [SerializeField] private MeshRenderer meshRenderer;

        private bool wasHit = false;

        private void OnEnable()
        {
            body.HitReceived += OnHitReceived;
        }

        private void OnDisable()
        {
            body.HitReceived -= OnHitReceived;
        }

        private void Update()
        {
            if (wasHit)
            {
                meshRenderer.material.color = HIT_COLOR;
                wasHit = false;
            }
            else
                meshRenderer.material.color = NOT_HIT_COLOR;
        }

        private void OnHitReceived(HitData3D data)
        {
            wasHit = true;
        }
    }
}
