using Shears.Logging;
using Shears.Tweens;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    public class MeshButton : UIElement
    {
        [Header("Mesh Button")]
        [SerializeField] private bool selectable = true;
        [SerializeField] private bool clickOnMouseDown = false;
        [SerializeField] private Renderer meshRenderer;
        [SerializeField] private Color hoverColor = new(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color pressedColor = new(0.4f, 0.4f, 0.4f);
        [SerializeField] private Color notSelectableColor = new(0.15f, 0.15f, 0.15f);

        [Header("Events")]
        [SerializeField] private UnityEvent clicked;

        private readonly StructTweenData notSelectableTweenData = new(0.1f, easingFunction: TweenEase.InOutQuad);
        private readonly List<TextMeshPro> textChildren = new();
        private ColorModulator colorModulator;
        private bool isFading = false;

        private ColorModulator ColorModulator
        {
            get
            {
                if (colorModulator == null || colorModulator.Renderer == null)
                    colorModulator = new(this, meshRenderer, hoverColor, pressedColor);

                return colorModulator;
            }
        }

        public bool IsHovered => colorModulator != null && colorModulator.IsHovered;
        public bool Selectable { get => selectable; set => SetSelectable(value); }

        public event Action Clicked;
        public event Action FadeInCompleted;
        public event Action FadeOutCompleted;

        protected override void Awake()
        {
            base.Awake();

            if (colorModulator == null || colorModulator.Renderer == null)
                colorModulator = new(this, meshRenderer, hoverColor, pressedColor);

            if (!selectable)
            {
                ColorModulator.SetColor(ColorModulator.OriginalColor * notSelectableColor);
                ColorModulator.CanChangeColor = false;
            }
        }

        [ContextMenu("Click")]
        public void Click()
        {
            OnClickedImplementation();
        }

        public void FadeIn(float duration = 0.5f, Color? targetColor = null, bool unscaledTime = false)
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

            Color resolvedColor = targetColor == null ? ColorModulator.OriginalColor : targetColor.Value;

            ColorModulator.CanChangeColor = true;
            ColorModulator.SetColor(ColorModulator.OriginalColor.With(a: 0.0f));
            ColorModulator.TweenToColor(resolvedColor, tweenData);
            ColorModulator.CanChangeColor = false;

            GetComponentsInChildren(true, textChildren);

            for (int i = 0; i < textChildren.Count; i++)
            {
                var child = textChildren[i];
                var childColor = child.color;

                child.color = childColor.With(a: 0.0f);
                child.DoColorTween(childColor, tweenData);
            }

            ColorModulator.AddOnComplete(() =>
            {
                selectable = wasSelectable;
                isFading = false;
                FadeInCompleted?.Invoke();

                ColorModulator.CanChangeColor = true;
            });
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

            ColorModulator.TweenToColor(ColorModulator.OriginalColor.With(a: 0.0f), tweenData);
            ColorModulator.CanChangeColor = false;

            GetComponentsInChildren(true, textChildren);

            for (int i = 0; i < textChildren.Count; i++)
            {
                var child = textChildren[i];
                var childColor = child.color;
                var targetColor = childColor.With(a: 0.0f);

                var childTween = child.DoColorTween(targetColor, tweenData);
                ColorModulator.AddOnComplete(() =>
                {
                    childTween.Dispose();
                    child.color = childColor;
                });
            }

            ColorModulator.AddOnComplete(() =>
            {
                selectable = wasSelectable;
                Disable();

                isFading = false;
                FadeOutCompleted?.Invoke();
            });
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
                    Color targetColor = IsHovered ? hoverColor : ColorModulator.OriginalColor;

                    ColorModulator.CanChangeColor = true;
                    ColorModulator.TweenToColor(targetColor, notSelectableTweenData);
                }
            }
            else
                ColorModulator.SetColor(ColorModulator.OriginalColor * notSelectableColor);
        }
    }
}
