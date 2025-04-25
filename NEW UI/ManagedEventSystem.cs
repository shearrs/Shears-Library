using Shears.Common;
using Shears.Input;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.UI
{
    public class ManagedEventSystem : ProtectedSingleton<ManagedEventSystem>
    {
        [SerializeField] private ManagedInputMap inputMap;
        [SerializeField] private ManagedUIElement firstFocused;

        private ManagedInputGroup inputs;
        private ManagedUIElement focus;
        private readonly Dictionary<string, ManagedUIElement> elements = new();

        private event Action<IReadOnlyCollection<ManagedUIElement>> InstOnNavigationChanged;

        internal static event Action<IReadOnlyCollection<ManagedUIElement>> OnNavigationChanged
        {
            add => Instance.InstOnNavigationChanged += value;
            remove => Instance.InstOnNavigationChanged -= value;
        }

        protected override void Awake()
        {
            base.Awake();

            Focus(firstFocused);
        }

        private void OnEnable()
        {
            inputs = inputMap.GetInputGroup(("Navigate",    ManagedInputPhase.Performed,    UpdateNavigation),
                                            ("Select",      ManagedInputPhase.Started,      BeginSelect),
                                            ("Select",      ManagedInputPhase.Canceled,     EndSelect));

            inputMap.EnableAllInputs();
            inputs.Bind();
        }

        private void OnDisable()
        {
            inputMap.DisableAllInputs();
            inputs.Unbind();
        }

        #region Element Events
        public void Focus(ManagedUIElement element)
        {
            if (focus != null && focus != element)
                focus.EndFocus();

            focus = element;

            if (focus != null)
                focus.BeginFocus();
        }
        #endregion

        #region Elements
        internal static void RegisterElement(ManagedUIElement element) => Instance.InstRegisterElement(element);
        private void InstRegisterElement(ManagedUIElement element)
        {
            if (elements.ContainsKey(element.ID))
            {
                Debug.LogWarning($"Element with ID {element.ID} already registered.");
                return;
            }

            elements[element.ID] = element;

            InstOnNavigationChanged?.Invoke(elements.Values);
        }

        internal static void DeregisterElement(ManagedUIElement element) => Instance.InstDeregisterElement(element);
        private void InstDeregisterElement(ManagedUIElement element)
        {
            if (!elements.Remove(element.ID))
            {
                Debug.LogWarning($"Element with ID {element.ID} not found.");
                return;
            }
            else
                InstOnNavigationChanged?.Invoke(elements.Values);
        }
        #endregion

        #region Input
        private void UpdateNavigation(ManagedInputInfo info)
        {
            Debug.Log("navigation: " + info.Input.ReadValue<Vector2>());
        }

        private void BeginSelect(ManagedInputInfo info)
        {
            if (focus == null)
                return;

            if (focus.Selectable)
                focus.BeginSelect();
        }

        private void EndSelect(ManagedInputInfo info)
        {
            if (focus == null)
                return;

            focus.EndSelect();
        }
        #endregion
    }
}
