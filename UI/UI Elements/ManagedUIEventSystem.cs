using Shears.Input;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    public class ManagedUIEventSystem : ProtectedSingleton<ManagedUIEventSystem>
    {
        private const string DEFAULT_INPUT_MAP_PATH = "ManagedElements/Shears_DefaultEventSystemInputMap";

        [SerializeField] private ManagedInputMap inputMap;
        [SerializeField] private ManagedUIElement firstFocused;

        private ManagedInputGroup inputs;
        private Vector2 previousNavigationInput;
        private ManagedUIElement focusedElement;
        private ManagedUIElement hoveredElement;
        private ManagedUIElement clickedElement;
        private ManagedUIElement selectedElement;
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

        protected override void OnInstanceCreated()
        {
            inputMap = Resources.Load<ManagedInputMap>(DEFAULT_INPUT_MAP_PATH);
        }

        #region Element Events
        public static void Focus(ManagedUIElement element) => Instance.InstFocus(element);
        private void InstFocus(ManagedUIElement element)
        {
            if (element == focusedElement)
                return;

            if (focusedElement != null)
                focusedElement.EndFocus();

            if (element == null || !element.Focusable)
            {
                focusedElement = null;
                return;
            }

            focusedElement = element;
            focusedElement.BeginFocus();
        }

        private void UpdateHoveredElement()
        {
            ManagedUIElement newHoverTarget = Raycast();

            if (newHoverTarget == hoveredElement)
                return;

            if (hoveredElement != null)
                hoveredElement.EndHover();

            // don't let us hover a new element if we are selecting something
            if (selectedElement != null && newHoverTarget != selectedElement)
            {
                hoveredElement = null;
                return;
            }

            hoveredElement = newHoverTarget;

            if (hoveredElement != null)
                hoveredElement.BeginHover();
        }

        private ManagedUIElement Raycast()
        {
            Vector2 pointerPos = ManagedPointer.Current.Position;
            List<Graphic> hitGraphics = new();
            ManagedUIElement hitElement = null;

            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);

            foreach (var canvas in canvases)
            {
                if (!canvas.TryGetComponent<GraphicRaycaster>(out _))
                    continue;

                var raycastableGraphics = GraphicRegistry.GetRaycastableGraphicsForCanvas(canvas);
                for (int i = 0; i < raycastableGraphics.Count; i++)
                {
                    var graphic = raycastableGraphics[i];

                    if (graphic == null || !graphic.raycastTarget)
                        continue;

                    if (RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPos))
                        hitGraphics.Add(graphic);
                }
            }

            int greatestDepth = int.MinValue;

            foreach (var graphic in hitGraphics)
            {
                if (graphic.depth > greatestDepth)
                {
                    greatestDepth = graphic.depth;
                    hitElement = GetManagedUIElement(graphic.gameObject);
                }
            }

            return hitElement;
        }

        private ManagedUIElement GetManagedUIElement(GameObject gameObject)
        {
            if (gameObject.TryGetComponent<ManagedUIElement>(out var element))
                return element;
            
            element = gameObject.GetComponentInParent<ManagedUIElement>();

            return element;
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
            if (focusedElement == null || selectedElement != null)
                return;

            var originalInput = info.Input.ReadValue<Vector2>();
            var input = originalInput;

            if (Mathf.Abs(previousNavigationInput.x) > 0)
                input.x = 0;
            if (Mathf.Abs(previousNavigationInput.y) > 0)
                input.y = 0;

            previousNavigationInput = originalInput;

            if (input == Vector2.zero)
                return;

            var direction = GetDirection(input);
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
            if (focusedElement == null || !focusedElement.Selectable)
                return;

            focusedElement.BeginSelect();
            selectedElement = focusedElement;
        }

        private void EndSelect(ManagedInputInfo info)
        {
            if (focusedElement == null)
                return;

            focusedElement.EndSelect();
            selectedElement = null;
        }

        private void BeginClick(ManagedInputInfo info)
        {
            if (hoveredElement == null)
                return;

            clickedElement = hoveredElement;
            selectedElement = clickedElement;

            clickedElement.BeginClick();

            Focus(clickedElement);
        }

        private void EndClick(ManagedInputInfo info)
        {
            if (clickedElement != null)
                clickedElement.EndClick();

            clickedElement = null;
            selectedElement = null;
        }
        #endregion
    }
}
