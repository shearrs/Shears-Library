using Shears.Tweens;
using System;
using TreeEditor;
using UnityEngine;

namespace Shears.UI
{
    public class ColorModulator : UIManipulator
    {
        #region Variables
        [Header("Color Modulator")]
        [SerializeField, RuntimeReadOnly]
        new private Renderer renderer;

        [SerializeField]
        private bool canChangeColor = true;

        [SerializeField, ReadOnly]
        private Color originalColor;

        [SerializeField]
        private Color hoverColor = new(0.6f, 0.6f, 0.6f, 1.0f);

        [SerializeField]
        private Color pressedColor = new(0.4f, 0.4f, 0.4f, 1.0f);

        private readonly TweenData hoverTweenData = new(0.1f, easingFunction: TweenEase.InOutQuad);
        private Tween tween;
        private Material material;
        private bool isHovered;
        private bool originalColorInitialized = false;

        public bool IsHovered => isHovered;
        public Renderer Renderer => renderer;
        public Color OriginalColor
        {
            get
            {
                if (!originalColorInitialized)
                    InitializeOriginalColor();

                return originalColor;
            }
        }
        public Color HoverColor { get => hoverColor; set => hoverColor = value; }
        public Color PressedColor { get => pressedColor; set => pressedColor = value; }
        public bool CanChangeColor { get => canChangeColor; set => canChangeColor = value; }
        #endregion

        protected override void Awake()
        {
            base.Awake();

            InitializeOriginalColor();
        }

        private void InitializeOriginalColor()
        {
            if (originalColorInitialized)
                return;

            if (renderer is SpriteRenderer spriteRenderer)
                originalColor = spriteRenderer.color.With(a: 1.0f);
            else
            {
                material = renderer.material;
                originalColor = material.color;
            }

            originalColorInitialized = true;
        }

        protected override void RegisterEvents()
        {
            Element.RegisterEvent<HoverEnterEvent>(OnHoverEnter);
            Element.RegisterEvent<HoverExitEvent>(OnHoverExit);
            Element.RegisterEvent<PointerDownEvent>(OnPointerDown);
            Element.RegisterEvent<PointerUpEvent>(OnPointerUp);
        }

        protected override void DeregisterEvents()
        {
            Element.DeregisterEvent<HoverEnterEvent>(OnHoverEnter);
            Element.DeregisterEvent<HoverExitEvent>(OnHoverExit);
            Element.DeregisterEvent<PointerDownEvent>(OnPointerDown);
            Element.DeregisterEvent<PointerUpEvent>(OnPointerUp);
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

            TweenToHover();
        }

        private void OnHoverExit(HoverExitEvent evt)
        {
            evt.PreventTrickleDown();

            isHovered = false;

            TweenToColor(originalColor, hoverTweenData);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            evt.PreventTrickleDown();

            TweenToPressed();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            evt.PreventTrickleDown();

            Color targetColor = isHovered ? hoverColor : originalColor;

            TweenToColor(targetColor, hoverTweenData);
        }

        public Tween FadeIn(ITweenData data)
        {
            if (renderer is SpriteRenderer spriteRenderer)
                tween = spriteRenderer.DoColorTween(spriteRenderer.color.With(a: 1.0f), data);
            else
                tween = material.DoColorTween(material.color.With(a: 1.0f), data);

            return tween;
        }

        public Tween FadeOut(ITweenData data)
        {
            if (renderer is SpriteRenderer spriteRenderer)
                tween = spriteRenderer.DoColorTween(spriteRenderer.color.With(a: 0.0f), data);
            else
                tween = material.DoColorTween(material.color.With(a: 0.0f), data);

            return tween;
        }

        public void ClearModulation() => TweenToColor(originalColor, hoverTweenData);

        public void TweenToHover() => TweenToColor(HoverColor, hoverTweenData);

        public void TweenToPressed() => TweenToColor(PressedColor, hoverTweenData);

        public void TweenToColor(Color color, ITweenData tweenData)
        {
            if (!canChangeColor)
                return;

            if (color != originalColor)
                color *= originalColor;

            tween.Dispose();

            if (renderer is SpriteRenderer spriteRenderer)
                tween = spriteRenderer.DoColorTween(color, tweenData);
            else
                tween = material.DoColorTween(color, tweenData);
        }

        public void AddOnComplete(Action action)
        {
            if (tween.IsValid)
                tween.Completed += action;
        }

        public void SetColor(Color color)
        {
            if (renderer is SpriteRenderer spriteRenderer)
                spriteRenderer.color = color;
            else
                material.color = color;
        }

        public void SetColor(float? r = null, float? g = null, float? b = null, float? a = null)
        {
            Color newColor = OriginalColor;

            if (r.HasValue)
                newColor.r = r.Value;
            if (g.HasValue)
                newColor.g = g.Value;
            if (b.HasValue)
                newColor.b = b.Value;
            if (a.HasValue)
                newColor.a = a.Value;
        }
    }
}
