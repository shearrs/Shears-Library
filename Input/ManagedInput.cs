using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shears.Input
{
    public enum ManagedInputPhase
    {
        Disabled = InputActionPhase.Disabled,
        Waiting = InputActionPhase.Waiting,
        Started = InputActionPhase.Started,
        Performed = InputActionPhase.Performed,
        Canceled = InputActionPhase.Canceled
    }

    public interface IManagedInput
    {
        public event InputEvent Started;
        public event InputEvent Performed;
        public event InputEvent Canceled;

        public void Enable();
        public void Disable();

        public void Register(ManagedInputPhase phase, InputEvent action);
        public void Deregister(ManagedInputPhase phase, InputEvent action);

        public T ReadValue<T>() where T : struct;
        public bool IsPressed();
        public bool WasPressedThisFrame();
    }

    public delegate void InputEvent(ManagedInputInfo inputInfo);

    public class ManagedInput : IManagedInput
    {
        private readonly InputAction inputAction;
        private readonly Dictionary<InputRegistration, Action<InputAction.CallbackContext>> registry = new();

        public string Name { get; private set; }

        #region Events
        public event InputEvent Started
        {
            add => Register(ManagedInputPhase.Started, value);
            remove => Deregister(ManagedInputPhase.Started, value);
        }
        public event InputEvent Performed
        {
            add => Register(ManagedInputPhase.Performed, value);
            remove => Deregister(ManagedInputPhase.Performed, value);
        }
        public event InputEvent Canceled
        {
            add => Register(ManagedInputPhase.Canceled, value);
            remove => Deregister(ManagedInputPhase.Canceled, value);
        }
        #endregion

        private struct InputRegistration
        {
            public ManagedInputPhase Phase { get; private set; }
            public InputEvent Action { get; private set; }

            public InputRegistration(ManagedInputPhase phase, InputEvent action)
                => (Phase, Action) = (phase, action);
        }

        public ManagedInput(InputAction action)
        {
            inputAction = action;
            Name = action.name;
        }

        public void Enable()
        {
            inputAction.Enable();
        }

        public void Disable()
        {
            inputAction.Disable();
        }
        
        public void Register(ManagedInputPhase phase, InputEvent action)
        {
            void callback(InputAction.CallbackContext ctx)
            {
                ManagedInputDevice device = new(ctx.control.device);
                ManagedInputInfo info = new(this, device);

                action?.Invoke(info);
            }

            InputRegistration registration = new(phase, action);

            switch (phase)
            {
                case ManagedInputPhase.Started:
                    inputAction.started += callback;
                    break;
                case ManagedInputPhase.Performed:
                    inputAction.performed += callback;
                    break;
                case ManagedInputPhase.Canceled:
                    inputAction.canceled += callback;
                    break;
            }

            registry.Add(registration, callback);
        }

        public void Deregister(ManagedInputPhase phase, InputEvent action)
        {
            InputRegistration registration = new(phase, action);

            if (!registry.TryGetValue(registration, out var callback))
                return;

            switch (phase)
            {
                case ManagedInputPhase.Started:
                    inputAction.started -= callback;
                    break;
                case ManagedInputPhase.Performed:
                    inputAction.performed -= callback;
                    break;
                case ManagedInputPhase.Canceled:
                    inputAction.canceled -= callback;
                    break;
            }

            registry.Remove(registration);
        }

        public T ReadValue<T>() where T : struct
        {
            return inputAction.ReadValue<T>();
        }

        public bool IsPressed()
        {
            return inputAction.IsPressed();
        }

        public bool WasPressedThisFrame()
        {
            return inputAction.WasPressedThisFrame();
        }
    }
}
