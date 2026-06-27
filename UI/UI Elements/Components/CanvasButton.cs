using System;
using Shears.Tweens;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    [RequireComponent(typeof(ColorModulator))]
    public class CanvasButton : UIElement
    {
        [Header("Mesh Button")]
        [SerializeField]
        private bool selectable = true;

        [SerializeField]
        private bool focusable = false;

        [SerializeField]
        private bool clickOnMouseDown = false;

        [SerializeField]
        private UIImage image;

        [SerializeField]
        private Color notSelectableColor = new(0.15f, 0.15f, 0.15f);

        [Header("Events")]
        [SerializeField]
        private UnityEvent clicked;

        private readonly TweenData notSelectableTweenData = new(
            0.1f,
            easingFunction: TweenEase.InOutQuad
        );
        private ColorModulator colorModulator;

        public bool InitializeColor { get; set; } = true;
        public ColorModulator ColorModulator
        {
            get
            {
                if (colorModulator == null)
                    colorModulator = GetComponent<ColorModulator>();

                return colorModulator;
            }
        }

        public UIImage Image
        {
            get => image;
            set => image = value;
        }

        public bool Selectable
        {
            get => selectable;
            set => SetSelectable(value);
        }

        public event Action Clicked;

        protected override void Awake()
        {
            base.Awake();

            if (!selectable)
                image.Modulate = notSelectableColor;
        }

        protected override void RegisterEvents()
        {
            RegisterEvent<PointerDownEvent>(OnPointerDown);
            RegisterEvent<ClickEvent>(OnClicked);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            evt.PreventTrickleDown();

            if (!selectable)
                return;

            if (clickOnMouseDown)
            {
                Clicked?.Invoke();
                clicked.Invoke();

                if (focusable)
                    Focus();
            }
        }

        private void OnClicked(ClickEvent evt)
        {
            evt.PreventTrickleDown();

            if (!selectable || clickOnMouseDown)
                return;

            Clicked?.Invoke();
            clicked.Invoke();

            if (focusable)
                Focus();
        }

        private void SetSelectable(bool value)
        {
            if (value == selectable)
                return;

            if (!IsFading)
            {
                if (isActiveAndEnabled)
                {
                    if (!value)
                        ColorModulator.TweenToColor(notSelectableColor, notSelectableTweenData);
                    else
                    {
                        if (IsHovered)
                            ColorModulator.TweenToHover();
                        else
                            ColorModulator.ClearModulation();
                    }
                }
                else
                    ColorModulator.ModulateColor(notSelectableColor);
            }

            selectable = value;
        }
    }
}
