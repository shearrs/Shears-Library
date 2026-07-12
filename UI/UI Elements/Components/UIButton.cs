using System;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    public class UIButton : UIElement
    {
        [Header("UI Button")]
        [SerializeField, Required, RuntimeReadOnly]
        private InteractableGraphicHandler graphicHandler = new();

        [Header("Settings")]
        [SerializeField]
        private Ref<bool> selectable = new(true);

        [SerializeField]
        private bool clickOnMouseDown = false;

        [Header("Events")]
        [SerializeField]
        private UnityEvent clicked;

        private readonly Ref<bool> isHovered = new();
        private readonly Ref<bool> isPressed = new();

        public bool IsHovered => isHovered.Value;
        public bool Selectable
        {
            get => selectable;
            set => SetSelectable(value);
        }

        public event Action Clicked;

        protected override void Awake()
        {
            base.Awake();

            graphicHandler.BindIsHovered(isHovered);
            graphicHandler.BindIsPressed(isPressed);
            graphicHandler.BindSelectable(selectable);
            graphicHandler.BindIsFocused(IsFocusedRef);

            if (!selectable)
                graphicHandler.InitializeNotSelectable();
        }

        private void Update()
        {
            graphicHandler.Update();
        }

        private void Reset()
        {
            InitializeDefaultGraphics();
        }

        private void OnValidate()
        {
            graphicHandler.Validate();
        }

        [ContextMenu("Click")]
        public void Click()
        {
            OnClickedImplementation();
        }

        protected override void RegisterEvents()
        {
            RegisterEvent<HoverEnterEvent>(OnHoverEnter);
            RegisterEvent<HoverExitEvent>(OnHoverExit);
            RegisterEvent<PointerDownEvent>(OnPointerDown);
            RegisterEvent<PointerUpEvent>(OnPointerUp);
            RegisterEvent<ClickEvent>(OnClicked);
        }

        public void AddGraphic(UIImage image)
        {
            graphicHandler.AddGraphic(image);
        }

        private void OnHoverEnter(HoverEnterEvent evt)
        {
            evt.PreventDefault();
            isHovered.Value = true;
        }

        private void OnHoverExit(HoverExitEvent evt)
        {
            evt.PreventDefault();
            isHovered.Value = false;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            evt.PreventDefault();

            if (!selectable)
                return;

            isPressed.Value = true;

            if (clickOnMouseDown)
                OnClickedImplementation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            evt.PreventDefault();

            isPressed.Value = false;
        }

        private void OnClicked(ClickEvent evt)
        {
            evt.PreventDefault();

            if (!selectable)
                return;

            OnClickedImplementation();
        }

        private void OnClickedImplementation()
        {
            Clicked?.Invoke();
            clicked.Invoke();
        }

        private void SetSelectable(bool value)
        {
            selectable.Value = value;

            if (!value)
                isPressed.Value = false;
        }

        private void InitializeDefaultGraphics()
        {
            var children = GetComponentsInChildren<UIElement>();
            foreach (var child in children)
            {
                if (child.TryGetComponent(out UIImage image))
                    graphicHandler.AddGraphic(image);
                else if (child.TryGetComponent(out UIText text))
                    graphicHandler.AddGraphic(text);
                else if (child.TryGetComponent(out UITextGUI textGUI))
                    graphicHandler.AddGraphic(textGUI);
                else if (child.TryGetComponent(out Renderer renderer))
                    graphicHandler.AddGraphic(renderer);
            }
        }
    }
}
