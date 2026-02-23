using Shears.Tweens;
using System;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

namespace Shears.UI
{
    public class ColorModulator : UIManipulator
    {
        #region Variables
        [Header("Color Modulator")]
        [SerializeField]
        private bool canChangeColor = true;

        [SerializeField, RuntimeReadOnly]
        private List<Renderer> renderers = new();

        [SerializeField]
        private Color hoverColor = new(0.6f, 0.6f, 0.6f, 1.0f);

        [SerializeField]
        private Color pressedColor = new(0.4f, 0.4f, 0.4f, 1.0f);

        private readonly Dictionary<Renderer, Color> originalColors = new();
        private readonly TweenData hoverTweenData = new(0.1f, easingFunction: TweenEase.InOutQuad);
        private readonly TweenStorage tweenStorage = new();
        private bool isHovered;
        private bool isDragged;
        private bool originalColorsInitialized = false;

        public bool IsHovered => isHovered;
        public bool IsDragged => isDragged;
        public List<Renderer> Renderers => renderers;
        public Color HoverColor { get => hoverColor; set => hoverColor = value; }
        public Color PressedColor { get => pressedColor; set => pressedColor = value; }
        public bool CanChangeColor { get => canChangeColor; set => canChangeColor = value; }
        #endregion

        protected override void Awake()
        {
            base.Awake();

            InitializeOriginalColors();
        }

        private void InitializeOriginalColors()
        {
            if (originalColorsInitialized)
                return;

            foreach (var renderer in renderers)
            {
                Color originalColor;

                if (renderer is SpriteRenderer spriteRenderer)
                    originalColor = spriteRenderer.color.With(a: 1.0f);
                else
                    originalColor = renderer.material.color;

                originalColors[renderer] = originalColor;
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

        [ContextMenu("Reset Colors")]
        private void ResetColors()
        {
            hoverColor = new(0.6f, 0.6f, 0.6f, 1.0f);
            pressedColor = new(0.4f, 0.4f, 0.4f, 1.0f);
        }

        private void OnHoverEnter(HoverEnterEvent evt)
        {
            evt.PreventTrickleDown();

            isHovered = true;

            if (isDragged)
                return;

            TweenToHover();
        }

        private void OnHoverExit(HoverExitEvent evt)
        {
            evt.PreventTrickleDown();

            isHovered = false;

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

            if (isHovered)
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

        public Tween FadeIn(ITweenData data)
        {
            foreach (var renderer in renderers)
            {
                if (renderer is SpriteRenderer spriteRenderer)
                    tweenStorage.Store(spriteRenderer.DoColorTween(spriteRenderer.color.With(a: 1.0f), data));
                else
                    tweenStorage.Store(renderer.material.DoColorTween(renderer.material.color.With(a: 1.0f), data));
            }

            return tweenStorage.Tweens[0];
        }

        public Tween FadeOut(ITweenData data)
        {
            foreach (var renderer in renderers)
            {
                if (renderer is SpriteRenderer spriteRenderer)
                    tweenStorage.Store(spriteRenderer.DoColorTween(spriteRenderer.color.With(a: 0.0f), data));
                else
                    tweenStorage.Store(renderer.material.DoColorTween(renderer.material.color.With(a: 0.0f), data));
            }

            return tweenStorage.Tweens[0];
        }

        public void ClearModulation()
        {
            if (!canChangeColor)
                return;

            tweenStorage.Dispose();

            foreach (var renderer in renderers)
            {
                var originalColor = originalColors[renderer];

                if (renderer is SpriteRenderer spriteRenderer)
                    tweenStorage.Store(spriteRenderer.DoColorTween(originalColor, hoverTweenData));
                else
                    renderer.material.DoColorTween(originalColor, hoverTweenData);
            }
        }

        public void TweenToHover() => TweenToColor(HoverColor, hoverTweenData);

        public void TweenToPressed() => TweenToColor(PressedColor, hoverTweenData);

        public void TweenToColor(Color color, ITweenData tweenData)
        {
            if (!canChangeColor)
                return;

            tweenStorage.Dispose();

            foreach (var renderer in renderers)
            {
                var originalColor = originalColors[renderer];

                if (color != originalColor)
                    color *= originalColor;

                if (renderer is SpriteRenderer spriteRenderer)
                    tweenStorage.Store(spriteRenderer.DoColorTween(color, tweenData));
                else
                    renderer.material.DoColorTween(color, tweenData);
            }
        }

        public void AddOnComplete(Action action)
        {
            if (tweenStorage.Tweens.Count > 0)
                tweenStorage.Tweens[0].Completed += action;
        }

        public void SetColor(Color color)
        {
            foreach (var renderer in renderers)
                SetColor(renderer, color);
        }

        public void SetColor(float? r = null, float? g = null, float? b = null, float? a = null)
        {
            foreach (var renderer in renderers)
            {
                Color newColor = originalColors[renderer];

                if (r.HasValue)
                    newColor.r = r.Value;
                if (g.HasValue)
                    newColor.g = g.Value;
                if (b.HasValue)
                    newColor.b = b.Value;
                if (a.HasValue)
                    newColor.a = a.Value;

                SetColor(renderer, newColor);
            }
        }

        public void ModulateColor(Color color)
        {
            foreach (var renderer in renderers)
                SetColor(renderer, originalColors[renderer] * color);
        }
        
        private void SetColor(Renderer renderer, Color color)
        {
            if (renderer is SpriteRenderer spriteRenderer)
                spriteRenderer.color = color;
            else
                renderer.material.color = color;
        }
    }
}
