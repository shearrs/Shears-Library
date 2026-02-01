using Shears.Logging;
using Shears.Tweens;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    public class CanvasButton : UIElement
    {
        [Header("Mesh Button")]
        [SerializeField] private bool selectable = true;
        [SerializeField] private bool clickOnMouseDown = false;
        [SerializeField] private bool usesUnscaledTime = false;
        [SerializeField] private ManagedImage image;
        [SerializeField] private Color hoverColor = new(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color pressedColor = new(0.4f, 0.4f, 0.4f);
        [SerializeField] private Color notSelectableColor = new(0.15f, 0.15f, 0.15f);

        [Header("Events")]
        [SerializeField] private UnityEvent clicked;

        private readonly TweenData hoverTweenData = new(0.1f, easingFunction: TweenEase.InOutQuad);
        private readonly TweenData notSelectableTweenData = new(0.1f, easingFunction: TweenEase.InOutQuad);
        private readonly List<TextMeshProUGUI> textChildren = new();
        private Tween tween;
        private bool isHovered = false;
        private bool isFading = false;

        public bool IsHovered => isHovered;
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

        public void FadeIn(float duration = 0.5f, Color? modulateColor = null)
        {
            if (isFading)
            {
                Log("Already fading!", SHLogLevels.Error);
                return;
            }

            isFading = true;
            var tweenData = new StructTweenData(duration, easingFunction: TweenEase.InOutQuad);

            Enable();
            bool wasSelectable = selectable;
            selectable = false;

            Color targetColor = modulateColor == null ? Color.white : modulateColor.Value;

            image.Modulate = image.Modulate.With(a: 0.0f);
            TweenToColor(targetColor, tweenData);

            GetComponentsInChildren(true, textChildren);
            for (int i = 0; i < textChildren.Count; i++)
            {
                var child = textChildren[i];
                var childColor = child.color;

                child.color = childColor.With(a: 0.0f);
                child.DoColorTween(childColor, tweenData);
            }

            tween.Completed += () =>
            {
                selectable = wasSelectable;
                isFading = false;
            };
        }

        public void FadeOut(float duration = 0.5f)
        {
            if (isFading)
            {
                Log("Already fading!", SHLogLevels.Error);
                return;
            }

            isFading = true;
            var tweenData = new StructTweenData(duration, easingFunction: TweenEase.InOutQuad);

            bool wasSelectable = selectable;
            selectable = false;

            TweenToColor(image.Modulate.With(a: 0.0f), tweenData);

            GetComponentsInChildren(true, textChildren);

            for (int i = 0; i < textChildren.Count; i++)
            {
                var child = textChildren[i];
                var childColor = child.color;
                var targetColor = childColor.With(a: 0.0f);

                var childTween = child.DoColorTween(targetColor, tweenData);
                tween.Completed += () =>
                {
                    childTween.Dispose();
                    child.color = childColor;
                };
            }

            tween.Completed += () =>
            {
                selectable = wasSelectable;
                Disable();

                isFading = false;
            };
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
            hoverTweenData.UnscaledTime = usesUnscaledTime;
            notSelectableTweenData.UnscaledTime = usesUnscaledTime;

            tween.Dispose();
            tween = image.DoModulateTween(color, tweenData);
        }

        private void SetSelectable(bool value)
        {
            if (value == selectable)
                return;

            if (isActiveAndEnabled)
            {
                if (selectable)
                {
                    TweenToColor(notSelectableColor, notSelectableTweenData);

                    if (isHovered)
                        HoverExited?.Invoke();
                }
                else
                {
                    Color targetColor = isHovered ? hoverColor : Color.white;

                    TweenToColor(targetColor, notSelectableTweenData);

                    if (isHovered)
                        HoverEntered?.Invoke();
                }
            }
            else
                image.Modulate = notSelectableColor;

            selectable = value;
        }
    }
}
