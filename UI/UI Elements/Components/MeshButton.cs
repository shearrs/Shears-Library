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
        [SerializeField] private Renderer meshRenderer;
        [SerializeField] private Color hoverColor = new(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color pressedColor = new(0.4f, 0.4f, 0.4f);
        [SerializeField] private Color notSelectableColor = new(0.15f, 0.15f, 0.15f);

        [Header("Events")]
        [SerializeField] private UnityEvent clicked;

        private readonly StructTweenData notSelectableTweenData = new(0.1f, easingFunction: TweenEase.InOutQuad);
        private ColorModulator colorModulator;
        private Material material;
        private Color originalColor;

        private Material Material
        {
            get
            {
                if (material == null)
                {
                    material = Instantiate(meshRenderer.material);
                    originalColor = material.color;
                    meshRenderer.material = material;
                }
                
                return material;
            }
        }

        public bool IsHovered => colorModulator != null && colorModulator.IsHovered;
        public bool Selectable { get => selectable; set => SetSelectable(value); }

        public event Action Clicked;

        protected override void Awake()
        {
            base.Awake();

            var material = Material;

            colorModulator = new(this, material, hoverColor, pressedColor);

            if (!selectable)
                material.color = originalColor * notSelectableColor;
        }

        protected override void RegisterEvents()
        {
            RegisterEvent<PointerDownEvent>(OnPointerDown);
            RegisterEvent<ClickEvent>(OnClicked);
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
        }

        private void OnClicked(ClickEvent evt)
        {
            if (!selectable || clickOnMouseDown)
                return;

            Clicked?.Invoke();
            clicked.Invoke();
        }
    
        private void SetSelectable(bool value)
        {
            if (value == selectable)
                return;

            if (isActiveAndEnabled)
            {
                if (selectable)
                    colorModulator.TweenToColor(notSelectableColor, notSelectableTweenData);
                else
                {
                    Color targetColor = IsHovered ? hoverColor : originalColor;

                    colorModulator.TweenToColor(targetColor, notSelectableTweenData);
                }
            }
            else
                Material.color = originalColor * notSelectableColor;

            selectable = value;
            colorModulator.CanChangeColor = selectable;
        }
    }
}
