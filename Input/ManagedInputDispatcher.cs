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
        public struct InputEvent
        {
            [Header("Input")]
            [SerializeField] private string inputName;
            [SerializeField] private ManagedInputPhase phase;
            [SerializeField] private bool isExpanded;

            [Header("Events")]
            [SerializeField] private UnityEvent<ManagedInputInfo> onInput;

            public readonly string InputName => inputName;
            public readonly ManagedInputPhase Phase => phase;

            public IManagedInput Input { get; set; }

            public readonly void Enable()
            {
                Input.Enable();
                Input.Bind(Phase, Invoke);
            }

            public readonly void Disable()
            {
                Input.Disable();
                Input.Unbind(Phase, Invoke);
            }

            private readonly void Invoke(ManagedInputInfo info)
            {
                onInput.Invoke(info);
            }
        }

        private void OnEnable()
        {
            if (initialized)
                return;

            for (int i = 0; i < inputEvents.Count; i++)
            {
                var evt = inputEvents[i];
                evt.Input = inputProvider.GetInput(inputEvents[i].InputName);

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
