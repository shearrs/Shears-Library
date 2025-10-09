using Shears.Tweens;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    public class MeshButton : UIElement
    {
        [Header("Mesh Button")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Color hoverColor = new(0.85f, 0.85f, 0.85f);
        [SerializeField] private Color pressedColor = new(0.5f, 0.5f, 0.5f);

        [Header("Events")]
        [SerializeField] private UnityEvent clicked;

        private readonly StructTweenData hoverTweenData = new(0.1f, easingFunction: EasingFunction.Ease.EaseInOutQuad);
        private Material material;
        private Color originalColor;
        private Tween tween;
        private bool isHovered = false;

        public event Action Clicked;

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
            RegisterEvent<ClickEvent>(OnClicked);
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

        private void OnClicked(ClickEvent evt)
        {
            Clicked?.Invoke();
            clicked.Invoke();
        }

        private void UpdateColor(Color start, Color end, float t)
        {
            material.color = Color.LerpUnclamped(start, end, t);
        }
    }
}
