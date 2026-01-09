using Shears.Tweens;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    [Serializable]
    public class ColorModulator
    {
        [SerializeField, RuntimeReadOnly] private readonly UIElement element;
        [SerializeField, RuntimeReadOnly] private readonly Material material;
        [SerializeField] private Color hoverColor = new(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color pressedColor = new(0.4f, 0.4f, 0.4f);
        [SerializeField] private Color originalColor;
        private readonly Func<bool> canChangeColor;
        private Tween tween;
        private bool isHovered;

        private readonly StructTweenData hoverTweenData = new(0.1f, easingFunction: TweenEase.InOutQuad);

        public bool IsHovered => isHovered;

        public ColorModulator(
            UIElement element, Func<bool> canChangeColor, Material material,
            Color? hoverColor = null, Color? pressedColor = null
        )
        {
            originalColor = material.color;

            this.element = element;
            this.canChangeColor = canChangeColor;
            this.material = material;
            this.hoverColor = hoverColor != null ? hoverColor.Value : new(0.6f, 0.6f, 0.6f);
            this.pressedColor = pressedColor != null ? pressedColor.Value : new(0.4f, 0.4f, 0.4f);

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
            isHovered = true;

            TweenToColor(hoverColor, hoverTweenData);
        }

        private void OnHoverExit(HoverExitEvent evt)
        {
            isHovered = false;

            TweenToColor(originalColor, hoverTweenData);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            TweenToColor(pressedColor, hoverTweenData);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            Color targetColor = isHovered ? hoverColor : originalColor;

            TweenToColor(targetColor, hoverTweenData);
        }

        public void TweenToColor(Color color, ITweenData tweenData)
        {
            if (!canChangeColor())
                return;

            if (color != originalColor)
                color *= originalColor;

            tween.Dispose();
            tween = material.DoColorTween(color, tweenData);
        }
    }
}
