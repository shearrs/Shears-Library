using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shears.Input
{
    [CreateAssetMenu(fileName = "New Managed Input Map", menuName = "Shears Library/Managed Input/Map")]
    public class ManagedInputMap : ManagedInputProvider
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName;

        private RuntimeInputMap runtimeMap;

        public ManagedInputUser User { get => runtimeMap.User; set => runtimeMap.User = value; }

        public override IManagedInput GetInput(string name) => GetRuntimeMap().GetInput(name);

        public override void GetInputs(params (string name, Action<IManagedInput> action)[] inputs)
            => GetRuntimeMap().GetInputs(inputs);

        public override ManagedInputGroup GetInputGroup(params (string name, ManagedInputPhase phase, ManagedInputEventWithInfo action)[] bindings)
            => GetRuntimeMap().GetInputGroup(bindings);

        public void EnableAllInputs() => GetRuntimeMap().EnableAllInputs();

        public void DisableAllInputs() => GetRuntimeMap().DisableAllInputs();

        public void DeregisterAllInputs(ManagedInputEventWithInfo action) => GetRuntimeMap().DeregisterAllInputs(action);

        private RuntimeInputMap GetRuntimeMap()
        {
            if (runtimeMap == null)
            {
                var gameObject = new GameObject("[Runtime] " + name)
                {
                    hideFlags = HideFlags.HideInHierarchy
                };
                runtimeMap = gameObject.AddComponent<RuntimeInputMap>();

                runtimeMap.Initialize(inputActions, actionMapName);
            }

            runtimeMap.EnableAllInputs();

            return runtimeMap;
        }

        public override ManagedInputType GetMostRecentInputType()
        {
            if (ManagedGamepad.Current.WasUpdatedThisFrame)
                return ManagedGamepad.Current;
            else
                return ManagedPointer.Current;
        }
    }
}
