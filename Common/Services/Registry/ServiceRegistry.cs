
using Shears.EarlyInitialization;
using Shears.Services;
using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    public class ServiceRegistry : MonoBehaviour, IEarlyInitializable
    {
        [SerializeField, RequiredField] private ServiceLocator locator;
        [SerializeField] private List<InterfaceReference<ISerializableService>> services;

        InitializationOrder IEarlyInitializable.Order => InitializationOrder.First;

        void IEarlyInitializable.EarlyInitialize()
        {
            foreach (var service in services)
            {
                service.Value.Register(locator);
            }
        }
    }
}
