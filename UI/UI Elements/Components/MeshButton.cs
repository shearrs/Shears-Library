using System;
using System.Collections.Generic;
using Shears.Logging;
using Shears.Tweens;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    [RequireComponent(typeof(ColorModulator))]
    public class MeshButton : UIElement
    {
        [Header("Mesh Button")]
        [SerializeField]
        private bool selectable = true;

        [SerializeField]
        private bool clickOnMouseDown = false;

        [SerializeField]
        private Color notSelectableColor = new(0.15f, 0.15f, 0.15f);

        [Header("Events")]
        [SerializeField]
        private UnityEvent clicked;

        private readonly StructTweenData notSelectableTweenData = new(
            0.1f,
            easingFunction: TweenEase.InOutQuad
        );
        private readonly List<TextMeshPro> textChildren = new();
        private readonly List<SpriteRenderer> spriteChildren = new();
        private readonly TweenStorage tweenStorage = new();
        private ColorModulator colorModulator;
        private bool isFadingIn = false;
        private bool isFadingOut = false;
        private bool initializeColor = true;

        public bool InitializeColor
        {
            get => initializeColor;
            set => initializeColor = value;
        }
        public ColorModulator ColorModulator
        {
            get
            {
                if (colorModulator == null)
                    colorModulator = GetComponent<ColorModulator>();

                return colorModulator;
            }
        }
        public bool Selectable
        {
            get => selectable;
            set => SetSelectable(value);
        }

        public event Action Clicked;
        public event Action FadeInCompleted;
        public event Action FadeOutCompleted;

        protected override void Awake()
        {
            base.Awake();

            if (!selectable && initializeColor)
            {
                ColorModulator.ModulateColor(notSelectableColor);
                ColorModulator.CanChangeColor = false;
            }
        }

        protected override void OnDisable()
        {
            tweenStorage.Dispose();
            isFadingIn = false;
            isFadingOut = false;

            base.OnDisable();
        }

        [ContextMenu("Click")]
        public void Click()
        {
            OnClickedImplementation();
        }

        public void FadeIn(float duration = 0.5f, bool unscaledTime = false)
        {
            if (isFadingIn)
                return;

            if (isFadingOut)
            {
                tweenStorage.Dispose();
                isFadingOut = false;
            }

            isFadingIn = true;
            var tweenData = new StructTweenData(
                duration,
                easingFunction: TweenEase.InOutQuad,
                unscaledTime: unscaledTime
            );

            Enable();
            bool wasSelectable = selectable;
            selectable = false;

            ColorModulator.CanChangeColor = true;
            ColorModulator.ModulateColor(Color.white.With(a: 0.0f));
            tweenStorage.Store(ColorModulator.FadeIn(tweenData));
            ColorModulator.CanChangeColor = false;

            GetComponentsInChildren(true, textChildren);
            GetComponentsInChildren(true, spriteChildren);

            foreach (var child in textChildren)
            {
                var childColor = child.color;

                child.color = childColor.With(a: 0.0f);
                tweenStorage.Store(child.DoColorTween(childColor.With(a: 1.0f), tweenData));
            }

            foreach (var child in spriteChildren)
            {
                var childColor = child.color;

                child.color = childColor.With(a: 0.0f);
                tweenStorage.Store(child.DoColorTween(childColor.With(a: 1.0f), tweenData));
            }

            ColorModulator.AddOnComplete(() =>
            {
                selectable = wasSelectable;
                isFadingIn = false;
                FadeInCompleted?.Invoke();

                ColorModulator.CanChangeColor = selectable;
            });
        }

        public void FadeOut(float duration = 0.5f, bool unscaledTime = false)
        {
            if (isFadingOut)
                return;

            if (isFadingIn)
            {
                tweenStorage.Dispose();
                isFadingIn = false;
            }

            isFadingOut = true;
            var tweenData = new StructTweenData(
                duration,
                easingFunction: TweenEase.InOutQuad,
                unscaledTime: unscaledTime
            );

            bool wasSelectable = selectable;
            selectable = false;

            tweenStorage.Store(ColorModulator.FadeOut(tweenData));
            ColorModulator.CanChangeColor = false;

            GetComponentsInChildren(true, textChildren);
            GetComponentsInChildren(true, spriteChildren);

            foreach (var child in textChildren)
            {
                var childColor = child.color;
                var targetColor = childColor.With(a: 0.0f);

                var childTween = child.DoColorTween(targetColor, tweenData);
                tweenStorage.Store(childTween);
                ColorModulator.AddOnComplete(() =>
                {
                    childTween.Dispose();
                    child.color = targetColor;
                });
            }

            foreach (var child in spriteChildren)
            {
                var childColor = child.color;
                var targetColor = childColor.With(a: 0.0f);

                var childTween = child.DoColorTween(targetColor, tweenData);
                tweenStorage.Store(childTween);
                ColorModulator.AddOnComplete(() =>
                {
                    childTween.Dispose();
                    child.color = targetColor;
                });
            }

            ColorModulator.AddOnComplete(() =>
            {
                selectable = wasSelectable;
                Disable();

                isFadingOut = false;
                FadeOutCompleted?.Invoke();
            });
        }

        public void SetAlpha(float alpha)
        {
            ColorModulator.SetColor(a: alpha);
            GetComponentsInChildren(true, textChildren);
            GetComponentsInChildren(true, spriteChildren);

            foreach (var child in textChildren)
                child.color = child.color.With(a: alpha);

            foreach (var child in spriteChildren)
                child.color = child.color.With(a: alpha);
        }

        protected override void RegisterEvents()
        {
            RegisterEvent<PointerDownEvent>(OnPointerDown);
            RegisterEvent<ClickEvent>(OnClicked);
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

        private void SetSelectable(bool value)
        {
            if (value == selectable)
                return;

            selectable = value;

            if (isActiveAndEnabled)
            {
                if (!selectable)
                {
                    ColorModulator.TweenToColor(notSelectableColor, notSelectableTweenData);
                    ColorModulator.CanChangeColor = false;
                }
                else
                {
                    ColorModulator.CanChangeColor = true;

                    if (IsHovered)
                        ColorModulator.TweenToHover();
                    else
                        ColorModulator.ClearModulation();
                }
            }
            else
                ColorModulator.ModulateColor(notSelectableColor);
        }
    }
}
