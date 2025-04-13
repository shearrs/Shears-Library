using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.Input
{
    public class ManagedInputDispatcher : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool enableOnStart = true;

        [Header("Inputs")]
        [SerializeField] private ManagedInputProvider inputProvider;
        [SerializeField] private List<InputEvent> inputEvents;

        private bool initialized = false;

        [System.Serializable]
        private struct InputEvent
        {
            [field: SerializeField] public string Name { get; private set; }
            [SerializeField] private UnityEvent onInput;

            public IManagedInput Input { get; set; }

            public readonly void Enable()
            {
                Input.Performed += Invoke;
            }

            public readonly void Disable()
            {
                Input.Performed -= Invoke;
            }

            private readonly void Invoke(ManagedInputInfo info)
            {
                onInput.Invoke();
            }
        }

        private void OnEnable()
        {
            if (initialized)
                return;

            for (int i = 0; i < inputEvents.Count; i++)
            {
                var evt = inputEvents[i];
                evt.Input = inputProvider.GetInput(inputEvents[i].Name);

                inputEvents[i] = evt;
            }

            initialized = true;
        }

        private void Start()
        {
            if (enableOnStart)
                Enable();
        }

        public void Enable()
        {
            foreach (var evt in inputEvents)
                evt.Enable();
        }

        public void Disable()
        {
            foreach (var evt in inputEvents)
                evt.Disable();
        }
    }
}
