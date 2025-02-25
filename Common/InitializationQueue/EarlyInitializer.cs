using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shears.EarlyInitialization
{
    [DefaultExecutionOrder(-3000)]
    public class EarlyInitializer : MonoBehaviour
    {
        private void Awake()
        {
            List<IEarlyInitializable> initializables = GetComponentsInChildren<IEarlyInitializable>().ToList();

            ExecuteOrder(initializables, InitializationOrder.First);
            ExecuteOrder(initializables, InitializationOrder.Second);
            ExecuteOrder(initializables, InitializationOrder.Third);
        }

        private void ExecuteOrder(List<IEarlyInitializable> initializables, InitializationOrder order)
        {
            foreach (var initializable in initializables)
            {
                if (initializable.Order == order)
                    initializable.EarlyInitialize();
            }
        }
    }
}
