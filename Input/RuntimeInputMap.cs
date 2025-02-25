using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shears.Input
{
    internal class RuntimeInputMap : MonoBehaviour
    {
        private InputActionAsset inputActions;
        private ManagedInputUser user;
        private Dictionary<string, ManagedInput> managedInputs;
        
        public ManagedInputUser User { get => user; set => SetUser(value); }

        public void Initialize(InputActionAsset inputActions, int actionMapIndex)
        {
            this.inputActions = Instantiate(inputActions);
            managedInputs = new();

            foreach (InputAction action in inputActions.actionMaps[actionMapIndex])
            {
                ManagedInput input = new(action);

                managedInputs.Add(action.name, input);
            }
        }

        public IManagedInput GetInput(string name)
        {
            if (managedInputs.TryGetValue(name, out ManagedInput input))
                return input;

            Debug.LogError($"Input: {name} not found in managed inputs!");
            return null;
        }

        public void EnableAllInputs()
        {
            foreach (var input in managedInputs.Values)
                input.Enable();
        }

        public void DisableAllInputs()
        {
            foreach (var input in managedInputs.Values)
                input.Disable();
        }

        public void DeregisterAllInputs(InputEvent action)
        {
            foreach (var input in managedInputs.Values)
                DeregisterAllPhases(input, action);
        }

        private void DeregisterAllPhases(IManagedInput input, InputEvent action)
        {
            input.Deregister(ManagedInputPhase.Disabled, action);
            input.Deregister(ManagedInputPhase.Waiting, action);
            input.Deregister(ManagedInputPhase.Started, action);
            input.Deregister(ManagedInputPhase.Performed, action);
            input.Deregister(ManagedInputPhase.Canceled, action);
        }

        public void SetUser(ManagedInputUser newUser)
        {
            user = newUser;

            inputActions.devices = new[] { user.Device.Device };
        }
    }
}
