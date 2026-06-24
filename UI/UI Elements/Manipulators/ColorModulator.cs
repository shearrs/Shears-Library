using System;
using System.Collections.Generic;
using Shears.Tweens;
using UnityEngine;

namespace Shears.UI
{
    public class ColorModulator : UIManipulator
    {
        private static readonly Color DEFAULT_HOVER_COLOR = new(0.6f, 0.6f, 0.6f, 1.0f);
        private static readonly Color DEFAULT_PRESSED_COLOR = new(0.4f, 0.4f, 0.4f, 1.0f);

        #region Variables
        [Header("Color Modulator")]
        [SerializeField]
        private bool canChangeColor = true;

        [SerializeField, RuntimeReadOnly]
        private List<RenderTarget> renderTargets = new();

        private readonly Dictionary<RenderTarget, Color> originalColors = new();
        private readonly TweenData tweenData = new(0.1f, easingFunction: TweenEase.InOutQuad);
        private readonly TweenStorage tweenStorage = new();
        private bool isDragged;
        private bool originalColorsInitialized = false;
        private Func<bool> canChangeColorCallback;

        private Dictionary<RenderTarget, Color> OriginalColors
        {
            get
            {
                if (!originalColorsInitialized)
                    InitializeOriginalColors();

                return originalColors;
            }
        }
        public bool IsDragged => isDragged;
        public List<RenderTarget> Renderers => renderTargets;
        public bool CanChangeColor
        {
            get => canChangeColor;
            set => canChangeColor = value;
        }
        public Func<bool> CanChangeColorCallback
        {
            get
            {
                canChangeColorCallback ??= () => canChangeColor;

                return canChangeColorCallback;
            }
            set => canChangeColorCallback = value;
        }
        #endregion

        [Serializable]
        public struct RenderTarget
        {
            public static readonly RenderTarget Default = new(
                DEFAULT_HOVER_COLOR,
                DEFAULT_PRESSED_COLOR
            );

            [SerializeField]
            public Renderer renderer;

            [SerializeField]
            public Color hoverColor;

            [SerializeField]
            public Color pressedColor;

            public RenderTarget(Color hoverColor, Color pressedColor)
            {
                renderer = null;
                this.hoverColor = hoverColor;
                this.pressedColor = pressedColor;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            InitializeOriginalColors();
        }

        private void OnValidate()
        {
            for (int i = 0; i < renderTargets.Count; i++)
            {
                if (renderTargets[i].hoverColor == Color.clear)
                    renderTargets[i] = RenderTarget.Default;
            }
        }

        private void InitializeOriginalColors()
        {
            if (originalColorsInitialized)
                return;

            foreach (var target in renderTargets)
            {
                Color originalColor;

                if (target.renderer is SpriteRenderer spriteRenderer)
                    originalColor = spriteRenderer.color.With(a: 1.0f);
                else
                    originalColor = target.renderer.material.color;

                originalColors[target] = originalColor;
            }

            originalColorsInitialized = true;
        }

        protected override void RegisterEvents()
        {
            Element.RegisterEvent<HoverEnterEvent>(OnHoverEnter);
            Element.RegisterEvent<HoverExitEvent>(OnHoverExit);
            Element.RegisterEvent<PointerDownEvent>(OnPointerDown);
            Element.RegisterEvent<PointerUpEvent>(OnPointerUp);
            Element.RegisterEvent<DragBeginEvent>(OnDragBegin);
            Element.RegisterEvent<DragEndEvent>(OnDragEnd);
        }

        protected override void DeregisterEvents()
        {
            Element.DeregisterEvent<HoverEnterEvent>(OnHoverEnter);
            Element.DeregisterEvent<HoverExitEvent>(OnHoverExit);
            Element.DeregisterEvent<PointerDownEvent>(OnPointerDown);
            Element.DeregisterEvent<PointerUpEvent>(OnPointerUp);
            Element.DeregisterEvent<DragBeginEvent>(OnDragBegin);
            Element.DeregisterEvent<DragEndEvent>(OnDragEnd);
        }

        private void OnHoverEnter(HoverEnterEvent evt)
        {
            evt.PreventTrickleDown();

            if (isDragged)
                return;

            TweenToHover();
        }

        private void OnHoverExit(HoverExitEvent evt)
        {
            evt.PreventTrickleDown();

            if (isDragged)
                return;

            ClearModulation();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            evt.PreventTrickleDown();

            if (isDragged)
                return;

            TweenToPressed();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            evt.PreventTrickleDown();

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

        public void ClearModulation()
        {
            if (!CanChangeColorCallback())
                return;

            tweenStorage.Dispose();

            foreach (var target in renderTargets)
            {
                var originalColor = OriginalColors[target];

                if (target.renderer is SpriteRenderer spriteRenderer)
                    tweenStorage.Store(spriteRenderer.DoColorTween(originalColor, tweenData));
                else
                    tweenStorage.Store(
                        target.renderer.material.DoColorTween(originalColor, tweenData)
                    );
            }
        }

        public void TweenToHover()
        {
            if (!CanChangeColorCallback())
                return;

            tweenStorage.Dispose();

            foreach (var target in renderTargets)
                TweenToColor(target, target.hoverColor, tweenData);
        }

        public void TweenToPressed()
        {
            if (!CanChangeColorCallback())
                return;

            tweenStorage.Dispose();

            foreach (var target in renderTargets)
                TweenToColor(target, target.pressedColor, tweenData);
        }

        public void TweenToColor(Color color, ITweenData tweenData)
        {
            if (!CanChangeColorCallback())
                return;

            tweenStorage.Dispose();

            foreach (var target in renderTargets)
                TweenToColor(target, color, tweenData);
        }

        public void AddOnComplete(Action action)
        {
            if (tweenStorage.HasValidTween())
                tweenStorage.GetFirstValid().Completed += action;
        }

        public void ModulateColor(Color color)
        {
            foreach (var target in renderTargets)
                SetColor(target, OriginalColors[target] * color);
        }

        private void TweenToColor(RenderTarget target, Color color, ITweenData tweenData)
        {
            var originalColor = OriginalColors[target];

            if (color != originalColor)
                color *= originalColor;

            if (target.renderer is SpriteRenderer spriteRenderer)
                tweenStorage.Store(spriteRenderer.DoColorTween(color, tweenData));
            else
                tweenStorage.Store(target.renderer.material.DoColorTween(color, tweenData));
        }

        private void SetColor(RenderTarget target, Color color)
        {
            if (target.renderer is SpriteRenderer spriteRenderer)
                spriteRenderer.color = color;
            else
                target.renderer.material.color = color;
        }
    }
}
