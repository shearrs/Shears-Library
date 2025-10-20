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
        [SerializeField] private Image image;
        [SerializeField] private Color hoverColor = new(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color pressedColor = new(0.4f, 0.4f, 0.4f);
        [SerializeField] private Color notSelectableColor = new(0.15f, 0.15f, 0.15f);

        [Header("Events")]
        [SerializeField] private UnityEvent clicked;

        private readonly StructTweenData hoverTweenData = new(0.1f, easingFunction: TweenEase.EaseInOutQuad);
        private readonly StructTweenData notSelectableTweenData = new(0.1f, easingFunction: TweenEase.EaseInOutQuad);
        private Color originalColor;
        private Tween tween;
        private bool isHovered = false;

        public bool Selectable { get => selectable; set => SetSelectable(value); }

        public event Action Clicked;

        protected override void Awake()
        {
            base.Awake();

            originalColor = image.color;

            if (!selectable)
                image.color = originalColor * notSelectableColor;
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
            Debug.Log("hover enter");

            if (!selectable)
                return;

            TweenToColor(hoverColor, hoverTweenData);
        }

        private void OnHoverExit(HoverExitEvent evt)
        {
            isHovered = false;
            Debug.Log("hover exit");

            if (!selectable)
                return;

            TweenToColor(originalColor, hoverTweenData);
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

            if (selectable)
                TweenToColor(pressedColor, hoverTweenData);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!selectable)
                return;

            Color targetColor = isHovered ? hoverColor : originalColor;

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
            if (color != originalColor)
                color *= originalColor;

            tween.Dispose();
            tween = TweenManager
                .DoTween((t) => UpdateColor(image.color, color, t), tweenData)
                .WithLifetime(this);
        }

        private void UpdateColor(Color start, Color end, float t)
        {
            image.color = Color.LerpUnclamped(start, end, t);
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
                    Color targetColor = isHovered ? hoverColor : originalColor;

                    TweenToColor(targetColor, notSelectableTweenData);
                }
            }
            else
                image.color = originalColor * notSelectableColor;

            selectable = value;
        }
    }
}
