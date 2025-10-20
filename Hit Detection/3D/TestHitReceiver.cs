using UnityEngine;

namespace Shears.HitDetection
{
    public class TestHitReceiver : HitReceiver3D
    {
        [SerializeField] private MeshRenderer render;

        private Material mat;
        private bool hitReceived = false;

        private void Awake()
        {
            mat = Instantiate(render.material);
            render.material = mat;

            HitReceived += Receive;
        }

        private void Update()
        {
            if (hitReceived)
            {
                render.material.color = Color.red;
                hitReceived = false;
            }
            else
                render.material.color = Color.green;
        }

        private void Receive(HitData3D _)
        {
            hitReceived = true;
        }
    }
}
