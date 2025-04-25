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

        private ManagedInputGroup inputs;
        private readonly Dictionary<string, ManagedUIElement> elements = new();

        private event Action<IReadOnlyCollection<ManagedUIElement>> onNavigationChanged;

        internal static event Action<IReadOnlyCollection<ManagedUIElement>> OnNavigationChanged
        {
            add => Instance.onNavigationChanged += value;
            remove => Instance.onNavigationChanged -= value;
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

            onNavigationChanged?.Invoke(elements.Values);
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
                onNavigationChanged?.Invoke(elements.Values);
        }
        #endregion

        #region Input
        private void UpdateNavigation(ManagedInputInfo info)
        {
            Debug.Log("navigation: " + info.Input.ReadValue<Vector2>());
        }

        private void BeginSelect(ManagedInputInfo info)
        {
            Debug.Log("begin select");
        }

        private void EndSelect(ManagedInputInfo info)
        {
            Debug.Log("end select");
        }
        #endregion
    }
}
