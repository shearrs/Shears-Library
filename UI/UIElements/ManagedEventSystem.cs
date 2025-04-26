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
        private ManagedUIElement focusedElement;
        private ManagedUIElement hoveredElement;
        private ManagedUIElement clickedElement;
        private readonly Dictionary<string, ManagedUIElement> elements = new();

        private event Action InstOnNavigationChanged;

        internal static IReadOnlyCollection<ManagedUIElement> Elements => Instance.elements.Values;

        internal static event Action OnNavigationChanged
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
            inputs ??= inputMap.GetInputGroup(("Navigate",      ManagedInputPhase.Performed,    Navigate),
                                              ("Select",        ManagedInputPhase.Started,      BeginSelect),
                                              ("Select",        ManagedInputPhase.Canceled,     EndSelect),
                                              ("Click",         ManagedInputPhase.Started,      BeginClick),
                                              ("Click",         ManagedInputPhase.Canceled,     EndClick));

            inputMap.EnableAllInputs();
            inputs.Bind();
        }

        private void OnDisable()
        {
            inputMap.DisableAllInputs();
            inputs.Unbind();
        }

        private void Update()
        {
            UpdateHoveredElement();
        }

        #region Element Events
        public static void Focus(ManagedUIElement element) => Instance.InstFocus(element);
        private void InstFocus(ManagedUIElement element)
        {
            if (element == focusedElement)
                return;

            if (focusedElement != null)
                focusedElement.EndFocus();

            focusedElement = element;

            if (focusedElement != null)
                focusedElement.BeginFocus();
        }

        private void UpdateHoveredElement()
        {
            Vector2 pointerPos = ManagedPointer.Current.Position;
            ManagedUIElement newHoverTarget = null;

            foreach (var element in elements.Values)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(element.RectTransform, pointerPos))
                {
                    newHoverTarget = element;
                    break;
                }
            }

            if (newHoverTarget == hoveredElement)
                return;

            if (hoveredElement != null)
                hoveredElement.EndHover();

            hoveredElement = newHoverTarget;

            if (hoveredElement != null)
                hoveredElement.BeginHover();
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

            InstOnNavigationChanged?.Invoke();
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
                InstOnNavigationChanged?.Invoke();
        }
        #endregion

        #region Input
        private void Navigate(ManagedInputInfo info)
        {
            if (focusedElement == null)
                return;

            var direction = GetDirection(info.Input.ReadValue<Vector2>());
            var newFocus = focusedElement.Navigate(direction);

            if (newFocus == null)
                return;

            Focus(newFocus);
        }

        private ManagedUINavigation.Direction GetDirection(Vector2 input)
        {
            if (input.x > 0)
                return ManagedUINavigation.Direction.Right;
            else if (input.x < 0)
                return ManagedUINavigation.Direction.Left;
            else if (input.y > 0)
                return ManagedUINavigation.Direction.Up;
            else if (input.y < 0)
                return ManagedUINavigation.Direction.Down;
            else
                return default;
        }

        private void BeginSelect(ManagedInputInfo info)
        {
            if (focusedElement == null)
                return;

            if (focusedElement.Selectable)
                focusedElement.BeginSelect();
        }

        private void EndSelect(ManagedInputInfo info)
        {
            if (focusedElement == null)
                return;

            focusedElement.EndSelect();
        }

        private void BeginClick(ManagedInputInfo info)
        {
            if (hoveredElement == null)
                return;

            clickedElement = hoveredElement;
            clickedElement.BeginClick();

            Focus(clickedElement);
        }

        private void EndClick(ManagedInputInfo info)
        {
            if (clickedElement != null)
                clickedElement.EndClick();

            clickedElement = null;
        }
        #endregion
    }
}
