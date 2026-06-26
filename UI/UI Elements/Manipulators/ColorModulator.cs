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
        private List<RenderTargetSettings> renderTargets = new();

        private readonly Dictionary<RenderTargetSettings, Color> originalColors = new();
        private readonly TweenData tweenData = new(0.1f, easingFunction: TweenEase.InOutQuad);
        private readonly TweenStorage tweenStorage = new();
        private bool isDragged;
        private bool originalColorsInitialized = false;
        private Func<bool> canChangeColorCallback;

        private Dictionary<RenderTargetSettings, Color> OriginalColors
        {
            get
            {
                if (!originalColorsInitialized)
                    InitializeOriginalColors();

                return originalColors;
            }
        }
        public bool IsDragged => isDragged;
        public List<RenderTargetSettings> Renderers => renderTargets;
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
        public struct RenderTargetSettings
        {
            public static readonly RenderTargetSettings Default = new(
                DEFAULT_HOVER_COLOR,
                DEFAULT_PRESSED_COLOR
            );

            [SerializeField]
            public UIElement.RenderTarget target;

            [SerializeField]
            public Color hoverColor;

            [SerializeField]
            public Color pressedColor;

            public RenderTargetSettings(Color hoverColor, Color pressedColor)
            {
                target = default;
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
                    renderTargets[i] = RenderTargetSettings.Default;
            }
        }

        private void InitializeOriginalColors()
        {
            if (originalColorsInitialized)
                return;

            foreach (var target in renderTargets)
                originalColors[target] = target.target.GetColor().With(a: 1.0f);

            originalColorsInitialized = true;
        }

        protected override void RegisterEvents()
        {
            Element.RegisterEvent<HoverEnterEvent>(OnHoverEnter);
            Element.RegisterEvent<HoverExitEvent>(OnHoverExit);
            Element.RegisterEvent<FocusEnterEvent>(OnFocusEnter);
            Element.RegisterEvent<FocusExitEvent>(OnFocusExit);
            Element.RegisterEvent<PointerDownEvent>(OnPointerDown);
            Element.RegisterEvent<PointerUpEvent>(OnPointerUp);
            Element.RegisterEvent<DragBeginEvent>(OnDragBegin);
            Element.RegisterEvent<DragEndEvent>(OnDragEnd);
        }

        protected override void DeregisterEvents()
        {
            Element.DeregisterEvent<HoverEnterEvent>(OnHoverEnter);
            Element.DeregisterEvent<HoverExitEvent>(OnHoverExit);
            Element.DeregisterEvent<FocusEnterEvent>(OnFocusEnter);
            Element.DeregisterEvent<FocusExitEvent>(OnFocusExit);
            Element.DeregisterEvent<PointerDownEvent>(OnPointerDown);
            Element.DeregisterEvent<PointerUpEvent>(OnPointerUp);
            Element.DeregisterEvent<DragBeginEvent>(OnDragBegin);
            Element.DeregisterEvent<DragEndEvent>(OnDragEnd);
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

        public void ClearModulation()
        {
            if (!CanChangeColorCallback())
                return;

            tweenStorage.Dispose();

            foreach (var target in renderTargets)
            {
                var originalColor = OriginalColors[target];

                tweenStorage.Store(target.target.DoColorTween(originalColor, tweenData));
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

        public void ModulateColor(Color color)
        {
            foreach (var target in renderTargets)
                target.target.SetColor(color);
        }

        private void TweenToColor(RenderTargetSettings target, Color color, ITweenData tweenData)
        {
            var originalColor = OriginalColors[target];

            if (color != originalColor)
                color *= originalColor;

            tweenStorage.Store(target.target.DoColorTween(color, tweenData));
        }
    }
}
