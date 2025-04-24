using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Input
{
    public class ManagedInputStack : MonoBehaviour
    {
        [SerializeField] private List<ManagedInputDispatcher> inputDispatchers = new();

        public void Add(ManagedInputDispatcher dispatcher)
        {
            if (inputDispatchers.Contains(dispatcher))
                return;

            inputDispatchers.Add(dispatcher);

            UpdateActiveDispatcher();
        }

        public void Remove(ManagedInputDispatcher dispatcher)
        {
            dispatcher.Disable();

            inputDispatchers.Remove(dispatcher);

            UpdateActiveDispatcher();
        }

        private void UpdateActiveDispatcher()
        {
            if (inputDispatchers.Count == 0)
                return;

            foreach (var dispatcher in inputDispatchers)
                dispatcher.Disable();

            inputDispatchers[^1].Enable();
        }
    }
}
