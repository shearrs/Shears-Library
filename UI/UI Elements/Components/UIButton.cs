using System;
using Shears.Tweens;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Audio.ProcessorInstance.AvailableData;

namespace Shears.UI
{
    public class UIButton : UIElement, IColorTweenable
    {
        private static readonly TweenData TWEEN_DATA = new(
            0.1f,
            easingFunction: TweenEase.InOutQuad
        );

        [Header("UI Button")]
        [SerializeField]
        private UIImage image;

        [SerializeField]
        private bool selectable = true;

        [SerializeField]
        private bool focusable = true;

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

        private bool isDragged;

        public Color BaseColor
        {
            get => image.BaseColor;
            set => image.BaseColor = value;
        }

        public Color Modulate
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

        [ContextMenu("Click")]
        public void Click()
        {
            OnClickedImplementation();
        }

        protected override void RegisterEvents()
        {
            RegisterEvent<PointerDownEvent>(OnPointerDown);
            RegisterEvent<ClickEvent>(OnClicked);
            RegisterEvent<HoverEnterEvent>(OnHoverEnter);
            RegisterEvent<HoverExitEvent>(OnHoverExit);
            RegisterEvent<FocusEnterEvent>(OnFocusEnter);
            RegisterEvent<FocusExitEvent>(OnFocusExit);
            RegisterEvent<PointerUpEvent>(OnPointerUp);
            RegisterEvent<DragBeginEvent>(OnDragBegin);
            RegisterEvent<DragEndEvent>(OnDragEnd);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            evt.PreventTrickleDown();

            if (!selectable)
                return;

            if (clickOnMouseDown)
                OnClickedImplementation();
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

        private void OnHoverEnter(HoverEnterEvent evt)
        {
            if (isDragged)
                return;

            TweenToHover();
        }

        private void OnHoverExit(HoverExitEvent evt)
        {
            if (isDragged)
                return;

            ClearModulation();
        }

        private void OnFocusEnter(FocusEnterEvent evt)
        {
            if (isDragged)
                return;

            TweenToHover();
        }

        private void OnFocusExit(FocusExitEvent evt)
        {
            if (isDragged)
                return;

            ClearModulation();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (isDragged)
                return;

            TweenToPressed();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (isDragged)
                return;

            if (Element.IsHovered)
                TweenToHover();
            else
                ClearModulation();
        }

        private void OnDragBegin(DragBeginEvent evt)
        {
            isDragged = true;

            TweenToHover();
        }

        private void OnDragEnd(DragEndEvent evt)
        {
            isDragged = false;
        }

        private void SetSelectable(bool value)
        {
            if (value == selectable)
                return;

            if (IsEnabled)
            {
                if (!value)
                    this.DoModulateTween(notSelectableColor.With(Modulate.a), TWEEN_DATA);
                else
                {
                    if (IsHovered)
                        this.DoModulateTween(hoverColor, TWEEN_DATA);
                    else
                        this.DoModulateTween(Color.white, TWEEN_DATA);
                }
            }
            else
                this.DoModulateTween(notSelectableColor, TWEEN_DATA);

            selectable = value;
        }
    }
}
