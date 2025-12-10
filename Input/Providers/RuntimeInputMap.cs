using System;
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

        public void Initialize(InputActionAsset inputActions, string actionMapName)
        {
            this.inputActions = Instantiate(inputActions);
            managedInputs = new();

            var actionMap = inputActions.FindActionMap(actionMapName, true);

            foreach (InputAction action in actionMap)
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

        public void GetInputs(params (string name, Action<IManagedInput> assignmentAction)[] inputs)
        {
            foreach (var input in inputs)
                input.assignmentAction(GetInput(input.name));
        }

        public ManagedInputGroup GetInputGroup(params (string name, ManagedInputPhase phase, ManagedInputEventWithInfo action)[] bindings)
        {
            ManagedInputBindingInstance[] bindingInstances = new ManagedInputBindingInstance[bindings.Length];
            
            for (int i = 0; i < bindings.Length; i++)
            {
                var (name, phase, action) = bindings[i];
                IManagedInput input = GetInput(name);

                if (input != null)
                    bindingInstances[i] = new(input, phase, action);
            }

            return new(bindingInstances);
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

        public void DeregisterAllInputs(ManagedInputEventWithInfo action)
        {
            foreach (var input in managedInputs.Values)
                DeregisterAllPhases(input, action);
        }

        private void DeregisterAllPhases(IManagedInput input, ManagedInputEventWithInfo action)
        {
            input.Unbind(ManagedInputPhase.Disabled, action);
            input.Unbind(ManagedInputPhase.Waiting, action);
            input.Unbind(ManagedInputPhase.Started, action);
            input.Unbind(ManagedInputPhase.Performed, action);
            input.Unbind(ManagedInputPhase.Canceled, action);
        }

        public void SetUser(ManagedInputUser newUser)
        {
            user = newUser;

            inputActions.devices = new[] { user.Device.Device };
        }
    }
}
