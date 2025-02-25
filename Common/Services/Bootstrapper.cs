using UnityEngine;

namespace Shears.Services
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ServiceLocator))]
    public abstract class Bootstrapper : MonoBehaviour
    {
        private ServiceLocator locator;
        private bool hasBeenBootstrapped;

        internal ServiceLocator Locator => locator.OrNull() ?? (locator = GetComponent<ServiceLocator>());

        private void Awake() => BootstrapOnDemand();

        public void BootstrapOnDemand()
        {
            if (hasBeenBootstrapped)
                return;

            Bootstrap();
            hasBeenBootstrapped = true;
        }

        protected abstract void Bootstrap();
    }
}
