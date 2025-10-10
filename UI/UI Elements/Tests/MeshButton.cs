using Shears.Tweens;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    public class MeshButton : UIElement
    {
        [Header("Mesh Button")]
        [SerializeField] private bool selectable = true;
        [SerializeField] private bool clickOnMouseDown = false;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Color hoverColor = new(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color pressedColor = new(0.4f, 0.4f, 0.4f);
        [SerializeField] private Color notSelectableColor = new(0.15f, 0.15f, 0.15f);

        [Header("Events")]
        [SerializeField] private UnityEvent clicked;

        private readonly StructTweenData hoverTweenData = new(0.1f, easingFunction: EasingFunction.Ease.EaseInOutQuad);
        private readonly StructTweenData notSelectableTweenData = new(0.1f, easingFunction: EasingFunction.Ease.EaseInOutQuad);
        private Material material;
        private Color originalColor;
        private Tween tween;
        private bool isHovered = false;

        private Material Material
        {
            get
            {
                if (material == null)
                {
                    material = Instantiate(meshRenderer.material);
                    meshRenderer.material = material;
                }
                
                return material;
            }
        }
        public bool Selectable { get => selectable; set => SetSelectable(value); }

        public event Action Clicked;

        protected override void Awake()
        {
            base.Awake();

            if (material == null)
            {
                material = Instantiate(meshRenderer.material);
                meshRenderer.material = material;
            }

            originalColor = material.color;

            if (!selectable)
                material.color = originalColor * notSelectableColor;
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
            isHovered = true;

            if (!selectable)
                return;

            TweenToColor(hoverColor, hoverTweenData);
        }

        private void OnHoverExit(HoverExitEvent evt)
        {
            isHovered = false;

            if (!selectable)
                return;

            TweenToColor(originalColor, hoverTweenData);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!selectable)
                return;

            if (clickOnMouseDown)
            {
                Clicked?.Invoke();
                clicked.Invoke();
            }

            TweenToColor(pressedColor, hoverTweenData);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!selectable)
                return;

            Color targetColor = isHovered ? hoverColor : originalColor;

            TweenToColor(targetColor, hoverTweenData);
        }

        private void OnClicked(ClickEvent evt)
        {
            if (!selectable || clickOnMouseDown)
                return;

            Clicked?.Invoke();
            clicked.Invoke();
        }

        private void TweenToColor(Color color, ITweenData tweenData)
        {
            if (color != originalColor)
                color *= originalColor;

            tween.Dispose();
            tween = TweenManager
                .DoTween((t) => UpdateColor(Material.color, color, t), tweenData)
                .WithLifetime(this);
        }

        private void UpdateColor(Color start, Color end, float t)
        {
            Material.color = Color.LerpUnclamped(start, end, t);
        }
    
        private void SetSelectable(bool value)
        {
            if (value == selectable)
                return;

            if (isActiveAndEnabled)
            {
                if (selectable)
                    TweenToColor(notSelectableColor, notSelectableTweenData);
                else
                {
                    Color targetColor = isHovered ? hoverColor : originalColor;

                    TweenToColor(targetColor, notSelectableTweenData);
                }
            }
            else
                Material.color = notSelectableColor;

            selectable = value;
        }
    }
}
