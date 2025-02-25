using UnityEngine;

namespace Shears.Services
{
    public class ServiceLocatorGlobalBootstrapper : Bootstrapper
    {
        [SerializeField] private bool dontDestroyOnLoad = true;

        protected override void Bootstrap()
        {
            Locator.ConfigureAsGlobal(dontDestroyOnLoad);
        }
    }
}
