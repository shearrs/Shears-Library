using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    public class UIButton : UIElement
    {
        private const float COLOR_MOVE_TIME = 0.1f;

        [Header("UI Button")]
        [SerializeField]
        private UIImage image;

        [SerializeField]
        private bool selectable = true;

        [SerializeField]
        private bool clickOnMouseDown = false;

        [Header("Colors")]
        [SerializeField]
        private Color hoverColor = new(0.6f, 0.6f, 0.6f, 1.0f);

        [SerializeField]
        private Color pressColor = new(0.4f, 0.4f, 0.4f, 1.0f);

        [SerializeField]
        private Color notSelectableColor = new(0.15f, 0.15f, 0.15f);

        [Header("Events")]
        [SerializeField]
        private UnityEvent clicked;

        private readonly Timer colorTimer = new(COLOR_MOVE_TIME);
        private Color startColor;
        private Color targetColor;
        private bool isDragged;
        private bool isPressed;

        public UIImage Image
        {
            get => image;
            set => image = value;
        }

        public override Color BaseColor
        {
            get => image.BaseColor;
            set => image.BaseColor = value;
        }

        public override Color Modulate
        {
            get => image.Modulate;
            set => image.Modulate = value;
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

            if (!selectable)
                Modulate = notSelectableColor.With(a: Modulate.a);
        }

        private void Update()
        {
            UpdateTargetColor();
        }

        [ContextMenu("Click")]
        public void Click()
        {
            OnClickedImplementation();
        }

        protected override void RegisterEvents()
        {
            RegisterEvent<PointerDownEvent>(OnPointerDown);
            RegisterEvent<PointerUpEvent>(OnPointerUp);
            RegisterEvent<ClickEvent>(OnClicked);
            RegisterEvent<DragBeginEvent>(OnDragBegin);
            RegisterEvent<DragEndEvent>(OnDragEnd);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            evt.PreventTrickleDown();

            if (!selectable)
                return;

            isPressed = true;

            if (clickOnMouseDown)
                OnClickedImplementation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            isPressed = false;
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
            isDragged = true;
        }

        private void OnDragEnd(DragEndEvent evt)
        {
            isDragged = false;
        }

        private void SetSelectable(bool value)
        {
            selectable = value;

            if (!value)
            {
                isPressed = false;
                isDragged = false;
            }
        }

        private void UpdateTargetColor()
        {
            Color newColor;

            if (!selectable)
                newColor = notSelectableColor;
            else
            {
                if (isPressed)
                    newColor = pressColor;
                else if (isDragged)
                    newColor = IsHovered ? pressColor : hoverColor;
                else if (IsHovered)
                    newColor = hoverColor;
                else if (IsFocused)
                    newColor = hoverColor;
                else
                    newColor = Color.white;
            }

            // If we are already this color, do nothing
            if (Modulate.CompareRGB(newColor))
                return;
            else if (targetColor != newColor || colorTimer.IsDone) // If this is a new color, or we aren't moving towards it, start moving toward it
            {
                colorTimer.Restart();
                targetColor = newColor;
                startColor = Modulate;
            }

            Modulate = Color
                .Lerp(startColor, targetColor, colorTimer.Percentage)
                .With(a: Modulate.a);
        }
    }
}
