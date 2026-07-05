using System;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    public class UIButton : UIElement
    {
        [Header("UI Button")]
        [SerializeField, Required, RuntimeReadOnly, Foldout(false)]
        private InteractableGraphicHandler graphicHandler = new();

        [SerializeField]
        private Ref<bool> selectable = new(true);

        [SerializeField]
        private bool clickOnMouseDown = false;

        [Header("Events")]
        [SerializeField]
        private UnityEvent clicked;

        private readonly Ref<bool> isHovered = new();
        private readonly Ref<bool> isPressed = new();
        private readonly Ref<bool> isDragged = new();
        private Color baseColor = Color.white;
        private Color modulate = Color.white;

        public bool IsHovered => isHovered.Value;
        public override Color BaseColor
        {
            get => baseColor;
            set => SetBaseColor(value);
        }
        public override Color Modulate
        {
            get => modulate;
            set => SetModulate(value);
        }
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
            graphicHandler.BindIsDragged(isDragged);
            graphicHandler.BindIsFocused(IsFocusedRef);

            if (!selectable)
                graphicHandler.InitializeNotSelectable();
        }

        private void Update()
        {
            graphicHandler.Update();
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
            RegisterEvent<DragBeginEvent>(OnDragBegin);
            RegisterEvent<DragEndEvent>(OnDragEnd);
        }

        public void AddGraphic(UIImage image)
        {
            graphicHandler.AddGraphic(image);
        }

        private void OnHoverEnter(HoverEnterEvent evt)
        {
            evt.PreventBubbleUp();

            isHovered.Value = true;
        }

        private void OnHoverExit(HoverExitEvent evt)
        {
            evt.PreventBubbleUp();

            isHovered.Value = false;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            evt.PreventTrickleDown();

            if (!selectable)
                return;

            isPressed.Value = true;

            if (clickOnMouseDown)
                OnClickedImplementation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            evt.PreventTrickleDown();

            isPressed.Value = false;
        }

        private void OnClicked(ClickEvent evt)
        {
            evt.PreventTrickleDown();

            if (!selectable)
                return;

            OnClickedImplementation();
        }

        private void OnClickedImplementation()
        {
            Clicked?.Invoke();
            clicked.Invoke();
        }

        private void OnDragBegin(DragBeginEvent evt)
        {
            isDragged.Value = true;
        }

        private void OnDragEnd(DragEndEvent evt)
        {
            isDragged.Value = false;
        }

        private void SetSelectable(bool value)
        {
            selectable.Value = value;

            if (!value)
            {
                isPressed.Value = false;
                isDragged.Value = false;
            }
        }

        private void SetBaseColor(Color value)
        {
            baseColor = value;

            graphicHandler.SetBaseColor(value);
        }

        private void SetModulate(Color value)
        {
            modulate = value;
            graphicHandler.SetModulate(value);
        }
    }
}
