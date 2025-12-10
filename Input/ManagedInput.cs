using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

using OldKey = UnityEngine.InputSystem.Key;

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
        public string Name { get; }
        public bool Enabled { get; }

        public event ManagedInputEvent Started;
        public event ManagedInputEvent Performed;
        public event ManagedInputEvent Canceled;
        public event ManagedInputEventWithInfo StartedWithInfo;
        public event ManagedInputEventWithInfo PerformedWithInfo;
        public event ManagedInputEventWithInfo CanceledWithInfo;

        public void Enable();
        public void Disable();

        public void Bind(ManagedInputPhase phase, ManagedInputEventWithInfo action);
        public void Unbind(ManagedInputPhase phase, ManagedInputEventWithInfo action);

        public T ReadValue<T>() where T : struct;
        public bool IsPressed();
        public bool WasPressedThisFrame();
    }

    public delegate void ManagedInputEvent();
    public delegate void ManagedInputEventWithInfo(ManagedInputInfo inputInfo);

    public class ManagedInput : IManagedInput
    {
        private readonly InputAction inputAction;
        private readonly Dictionary<IManagedInputBinding, Action<InputAction.CallbackContext>> bindings = new();

        public string Name { get; private set; }
        public bool Enabled => inputAction.enabled;

        #region Events
        public event ManagedInputEvent Started
        {
            add => Bind(ManagedInputPhase.Started, value);
            remove => Unbind(ManagedInputPhase.Started, value);
        }
        public event ManagedInputEvent Performed
        {
            add => Bind(ManagedInputPhase.Performed, value);
            remove => Unbind(ManagedInputPhase.Performed, value);
        }
        public event ManagedInputEvent Canceled
        {
            add => Bind(ManagedInputPhase.Canceled, value);
            remove => Unbind(ManagedInputPhase.Canceled, value);
        }

        public event ManagedInputEventWithInfo StartedWithInfo
        {
            add => Bind(ManagedInputPhase.Started, value);
            remove => Unbind(ManagedInputPhase.Started, value);
        }
        public event ManagedInputEventWithInfo PerformedWithInfo
        {
            add => Bind(ManagedInputPhase.Performed, value);
            remove => Unbind(ManagedInputPhase.Performed, value);
        }
        public event ManagedInputEventWithInfo CanceledWithInfo
        {
            add => Bind(ManagedInputPhase.Canceled, value);
            remove => Unbind(ManagedInputPhase.Canceled, value);
        }
        #endregion

        public ManagedInput(InputAction action)
        {
            inputAction = action;
            Name = action.name;
        }

        ~ManagedInput()
        {
            foreach (var binding in bindings.Keys)
            {
                switch (binding.Phase)
                {
                    case ManagedInputPhase.Started:
                        inputAction.started -= bindings[binding];
                        break;
                    case ManagedInputPhase.Performed:
                        inputAction.performed -= bindings[binding];
                        break;
                    case ManagedInputPhase.Canceled:
                        inputAction.canceled -= bindings[binding];
                        break;
                }
            }
        }

        public void Enable()
        {
            inputAction.Enable();
        }

        public void Disable()
        {
            inputAction.Disable();
        }

        public void Bind(ManagedInputPhase phase, ManagedInputEvent action)
        {
            void callback(InputAction.CallbackContext ctx)
            {
                action?.Invoke();
            }

            var binding = new ManagedInputBinding(phase, action);

            AddBinding(binding, callback);
        }

        public void Bind(ManagedInputPhase phase, ManagedInputEventWithInfo action)
        {
            void callback(InputAction.CallbackContext ctx)
            {
                ManagedInputDevice device = new(ctx.control.device);
                ManagedInputInfo info = new(this, GetManagedPhase(ctx.phase), device);

                action?.Invoke(info);
            }

            var binding = new ManagedInputBindingWithInfo(phase, action);

            AddBinding(binding, callback);
        }

        public void Unbind(ManagedInputPhase phase, ManagedInputEvent action)
        {
            var binding = new ManagedInputBinding(phase, action);

            RemoveBinding(binding);
        }

        public void Unbind(ManagedInputPhase phase, ManagedInputEventWithInfo action)
        {
            var binding = new ManagedInputBindingWithInfo(phase, action);

            RemoveBinding(binding);
        }

        public T ReadValue<T>() where T : struct
        {
            if (typeof(T) == typeof(KeyControl))
                return (T)(object)KeyTranslation.TranslateKey(inputAction.ReadValue<OldKey>());

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

        private ManagedInputPhase GetManagedPhase(InputActionPhase actionPhase)
        {
            return actionPhase switch
            {
                InputActionPhase.Started => ManagedInputPhase.Started,
                InputActionPhase.Performed => ManagedInputPhase.Performed,
                InputActionPhase.Canceled => ManagedInputPhase.Canceled,
                InputActionPhase.Disabled => ManagedInputPhase.Disabled,
                InputActionPhase.Waiting => ManagedInputPhase.Waiting,
                _ => default,
            };
        }

        private void AddBinding(IManagedInputBinding binding, Action<InputAction.CallbackContext> callback)
        {
            switch (binding.Phase)
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

            bindings.Add(binding, callback);
        }

        private void RemoveBinding(IManagedInputBinding binding)
        {
            if (!bindings.TryGetValue(binding, out var callback))
                return;

            switch (binding.Phase)
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

            bindings.Remove(binding);
        }
    }
}
