using UnityEngine;

namespace Shears
{
    public class TimedInstance : MonoBehaviour
    {
        [SerializeField, Min(0.01f)]
        private float lifetime = 1.0f;

        public float Lifetime
        {
            get => lifetime;
            set => lifetime = value;
        }

        private void Awake()
        {
            CoroutineUtil.DoAfter(() => Destroy(gameObject), lifetime, this);
        }
    }
}
