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
        [SerializeField] private bool focusable = false;
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
        private bool isFocused = false;
        private bool isHovered = false;
        private bool isFading = false;

        public bool IsFocused => isFocused;
        public bool IsHovered => isHovered;
        public ManagedImage Image { get => image; set => image = value; }
        public bool Selectable { get => selectable; set => SetSelectable(value); }

        public event Action Clicked;
        public event Action PointerDown;
        public event Action PointerUp;
        public event Action Focused;
        public event Action Unfocused;
        public event Action HoverEntered;
        public event Action HoverExited;
        public event Action FadeInCompleted;
        public event Action FadeOutCompleted;

        protected override void Awake()
        {
            base.Awake();

            if (!selectable)
                image.Modulate = notSelectableColor;
        }

        protected override void RegisterEvents()
        {
            RegisterEvent<FocusEnterEvent>(OnFocusEnter);
            RegisterEvent<FocusExitEvent>(OnFocusExit);
            RegisterEvent<HoverEnterEvent>(OnHoverEnter);
            RegisterEvent<HoverExitEvent>(OnHoverExit);
            RegisterEvent<PointerDownEvent>(OnPointerDown);
            RegisterEvent<PointerUpEvent>(OnPointerUp);
            RegisterEvent<ClickEvent>(OnClicked);
        }

        public void FadeIn(float duration = 0.5f, Color? modulateColor = null, bool unscaledTime = false)
        {
            if (isFading)
            {
                Log("Already fading!", SHLogLevels.Error);
                return;
            }

            isFading = true;
            var tweenData = new StructTweenData(duration, easingFunction: TweenEase.InOutQuad, unscaledTime: unscaledTime);

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
                FadeInCompleted?.Invoke();
            };
        }

        public void FadeOut(float duration = 0.5f, bool unscaledTime = false)
        {
            if (isFading)
            {
                Log("Already fading!", SHLogLevels.Error);
                return;
            }

            isFading = true;
            var tweenData = new StructTweenData(duration, easingFunction: TweenEase.InOutQuad, unscaledTime: unscaledTime);

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
                FadeOutCompleted?.Invoke();
            };
        }

        private void OnFocusEnter(FocusEnterEvent evt)
        {
            evt.PreventTrickleDown();
            isFocused = true;

            if (!selectable)
                return;

            Focused?.Invoke();
            TweenToColor(hoverColor, hoverTweenData);
        }

        private void OnFocusExit(FocusExitEvent evt)
        {
            evt.PreventTrickleDown();
            isFocused = false;

            if (!selectable)
                return;

            Unfocused?.Invoke();
            TweenToColor(Color.white, hoverTweenData);
        }

        private void OnHoverEnter(HoverEnterEvent evt)
        {
            evt.PreventTrickleDown();
            isHovered = true;

            if (!selectable)
                return;

            HoverEntered?.Invoke();
            TweenToColor(hoverColor, hoverTweenData);
        }

        private void OnHoverExit(HoverExitEvent evt)
        {
            evt.PreventTrickleDown();
            isHovered = false;

            if (!selectable)
                return;

            HoverExited?.Invoke();
            TweenToColor(Color.white, hoverTweenData);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            evt.PreventTrickleDown();

            if (!selectable)
                return;

            if (clickOnMouseDown)
            {
                Clicked?.Invoke();
                clicked.Invoke();

                if (focusable)
                    Focus();
            }

            PointerDown?.Invoke();

            if (selectable)
                TweenToColor(pressedColor, hoverTweenData);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            evt.PreventTrickleDown();

            if (!selectable)
                return;

            Color targetColor = isHovered ? hoverColor : Color.white;

            PointerUp?.Invoke();

            TweenToColor(targetColor, hoverTweenData);
        }

        private void OnClicked(ClickEvent evt)
        {
            evt.PreventTrickleDown();

            if (!selectable || clickOnMouseDown)
                return;

            Clicked?.Invoke();
            clicked.Invoke();

            if (focusable)
                Focus();
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
