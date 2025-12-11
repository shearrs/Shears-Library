using Shears.Tweens;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Shears.UI
{
    public class CanvasButton : UIElement
    {
        [Header("Mesh Button")]
        [SerializeField] private bool selectable = true;
        [SerializeField] private bool clickOnMouseDown = false;
        [SerializeField] private ManagedImage image;
        [SerializeField] private Color hoverColor = new(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color pressedColor = new(0.4f, 0.4f, 0.4f);
        [SerializeField] private Color notSelectableColor = new(0.15f, 0.15f, 0.15f);

        [Header("Events")]
        [SerializeField] private UnityEvent clicked;

        private readonly StructTweenData hoverTweenData = new(0.1f, easingFunction: TweenEase.InOutQuad);
        private readonly StructTweenData notSelectableTweenData = new(0.1f, easingFunction: TweenEase.InOutQuad);
        private Tween tween;
        private bool isHovered = false;

        public ManagedImage Image { get => image; set => image = value; }
        public bool Selectable { get => selectable; set => SetSelectable(value); }

        public event Action Clicked;
        public event Action PointerDown;
        public event Action PointerUp;
        public event Action HoverEntered;
        public event Action HoverExited;

        protected override void Awake()
        {
            base.Awake();

            if (!selectable)
                image.Modulate = notSelectableColor;
        }

        protected override void RegisterEvents()
        {
            RegisterEvent<HoverEnterEvent>(OnHoverEnter);
            RegisterEvent<HoverExitEvent>(OnHoverExit);
            RegisterEvent<PointerDownEvent>(OnPointerDown);
            RegisterEvent<PointerUpEvent>(OnPointerUp);
            RegisterEvent<ClickEvent>(OnClicked);
        }

        private void OnHoverEnter(HoverEnterEvent evt)
        {
            isHovered = true;

            if (!selectable)
                return;

            HoverEntered?.Invoke();
            TweenToColor(hoverColor, hoverTweenData);
        }

        private void OnHoverExit(HoverExitEvent evt)
        {
            isHovered = false;

            if (!selectable)
                return;

            HoverExited?.Invoke();
            TweenToColor(Color.white, hoverTweenData);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!selectable)
                return;

            if (clickOnMouseDown)
            {
                Clicked?.Invoke();
                clicked.Invoke();
            }

            PointerDown?.Invoke();

            if (selectable)
                TweenToColor(pressedColor, hoverTweenData);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!selectable)
                return;

            Color targetColor = isHovered ? hoverColor : Color.white;

            PointerUp?.Invoke();

            TweenToColor(targetColor, hoverTweenData);
        }

        private void OnClicked(ClickEvent evt)
        {
            if (!selectable || clickOnMouseDown)
                return;

            Clicked?.Invoke();
            clicked.Invoke();
        }

        private void TweenToColor(Color color, ITweenData tweenData)
        {
            tween.Dispose();
            tween = TweenManager
                .DoTween((t) => UpdateColor(image.Modulate, color, t), tweenData)
                .WithLifetime(this);
        }

        private void UpdateColor(Color start, Color end, float t)
        {
            image.Modulate = Color.LerpUnclamped(start, end, t);
        }

        private void SetSelectable(bool value)
        {
            if (value == selectable)
                return;

            if (isActiveAndEnabled)
            {
                if (selectable)
                    TweenToColor(notSelectableColor, notSelectableTweenData);
                else
                {
                    Color targetColor = isHovered ? hoverColor : Color.white;

                    TweenToColor(targetColor, notSelectableTweenData);
                }
            }
            else
                image.Modulate = notSelectableColor;

            selectable = value;
        }
    }
}
