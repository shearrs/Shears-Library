using Shears.Tweens;
using System;
using UnityEngine;

namespace Shears.UI
{
    public class MeshButton : UIElement
    {
        [Header("Mesh Button")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Color hoverColor = new(0.85f, 0.85f, 0.85f);
        [SerializeField] private Color pressedColor = new(0.5f, 0.5f, 0.5f);

        private readonly StructTweenData hoverTweenData = new(0.1f, easingFunction: EasingFunction.Ease.EaseInOutQuad);
        private Material material;
        private Color originalColor;
        private Tween tween;
        private bool isHovered = false;

        protected override void Awake()
        {
            base.Awake();

            material = meshRenderer.material;
            originalColor = material.color;
        }

        protected override void RegisterEvents()
        {
            RegisterEvent<HoverEnterEvent>(OnHoverEnter);
            RegisterEvent<HoverExitEvent>(OnHoverExit);
            RegisterEvent<PointerDownEvent>(OnPointerDown);
            RegisterEvent<PointerUpEvent>(OnPointerUp);
        }

        private void OnHoverEnter(HoverEnterEvent evt)
        {
            tween.Dispose();
            
            tween = TweenManager
                .DoTween((t) => UpdateColor(material.color, originalColor * hoverColor, t), hoverTweenData)
                .WithLifetime(this);

            isHovered = true;
        }

        private void OnHoverExit(HoverExitEvent evt)
        {
            tween.Dispose();
            tween = TweenManager
                .DoTween((t) => UpdateColor(material.color, originalColor, t), hoverTweenData)
                .WithLifetime(this);

            isHovered = false;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            tween.Dispose();
            tween = TweenManager
                .DoTween((t) => UpdateColor(material.color, originalColor * pressedColor, t), hoverTweenData)
                .WithLifetime(this);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            Color targetColor = isHovered ? originalColor * hoverColor : originalColor;

            tween.Dispose();
            tween = TweenManager
                .DoTween((t) => UpdateColor(material.color, targetColor, t), hoverTweenData)
                .WithLifetime(this);
        }

        private void UpdateColor(Color start, Color end, float t)
        {
            material.color = Color.LerpUnclamped(start, end, t);
        }
    }
}
