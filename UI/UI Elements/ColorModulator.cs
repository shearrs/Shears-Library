using Shears.Tweens;
using System;
using UnityEngine;

namespace Shears.UI
{
    [Serializable]
    public class ColorModulator
    {
        [SerializeField, RuntimeReadOnly]
        private readonly UIElement element;

        [SerializeField, RuntimeReadOnly]
        private readonly Renderer renderer;

        [SerializeField, RuntimeReadOnly]
        private readonly Material material;

        [SerializeField]
        private bool canChangeColor = true;        

        [SerializeField]
        private Color hoverColor = new(0.6f, 0.6f, 0.6f, 1.0f);

        [SerializeField]
        private Color pressedColor = new(0.4f, 0.4f, 0.4f, 1.0f);

        private Color originalColor;
        private Tween tween;
        private bool isHovered;

        private readonly TweenData hoverTweenData = new(0.1f, easingFunction: TweenEase.InOutQuad);

        public bool IsHovered => isHovered;
        public Renderer Renderer => renderer;
        public Color OriginalColor => originalColor;
        public Color HoverColor { get => hoverColor; set => hoverColor = value; }
        public Color PressedColor { get => pressedColor; set => pressedColor = value; }
        public bool CanChangeColor { get => canChangeColor; set => canChangeColor = value; }

        public ColorModulator(
            UIElement element, Renderer renderer,
            Color? hoverColor = null, Color? pressedColor = null
        )
        {
            this.renderer = renderer;

            if (renderer is SpriteRenderer spriteRenderer)
                originalColor = spriteRenderer.color.With(a: 1.0f);
            else
            {
                material = renderer.material;
                originalColor = material.color;
            }

            this.element = element;
            this.hoverColor = hoverColor != null ? hoverColor.Value : new(0.6f, 0.6f, 0.6f, 1.0f);
            this.pressedColor = pressedColor != null ? pressedColor.Value : new(0.4f, 0.4f, 0.4f, 1.0f);

            element.RegisterEvent<HoverEnterEvent>(OnHoverEnter);
            element.RegisterEvent<HoverExitEvent>(OnHoverExit);
            element.RegisterEvent<PointerDownEvent>(OnPointerDown);
            element.RegisterEvent<PointerUpEvent>(OnPointerUp);
        }

        ~ColorModulator()
        {
            element.DeregisterEvent<HoverEnterEvent>(OnHoverEnter);
            element.DeregisterEvent<HoverExitEvent>(OnHoverExit);
            element.DeregisterEvent<PointerDownEvent>(OnPointerDown);
            element.DeregisterEvent<PointerUpEvent>(OnPointerUp);
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
