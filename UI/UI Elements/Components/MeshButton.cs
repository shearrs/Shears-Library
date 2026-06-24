using System;
using System.Collections.Generic;
using Shears.Tweens;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    [RequireComponent(typeof(ColorModulator))]
    public class MeshButton : UIElement
    {
        [Header("Mesh Button")]
        [SerializeField]
        private bool selectable = true;

        [SerializeField]
        private bool clickOnMouseDown = false;

        [SerializeField]
        private Color notSelectableColor = new(0.15f, 0.15f, 0.15f);

        [Header("Events")]
        [SerializeField]
        private UnityEvent clicked;

        private readonly StructTweenData notSelectableTweenData = new(
            0.1f,
            easingFunction: TweenEase.InOutQuad
        );
        private ColorModulator colorModulator;
        private bool initializeColor = true;

        public bool InitializeColor
        {
            get => initializeColor;
            set => initializeColor = value;
        }
        public ColorModulator ColorModulator
        {
            get
            {
                if (colorModulator == null)
                    colorModulator = GetComponent<ColorModulator>();

                return colorModulator;
            }
        }
        public bool Selectable
        {
            get => GetSelectable();
            set => SetSelectable(value);
        }

        public event Action Clicked;

        protected override void Awake()
        {
            base.Awake();

            ColorModulator.CanChangeColorCallback = GetSelectable;

            if (!selectable && initializeColor)
                ColorModulator.ModulateColor(notSelectableColor);
        }

        [ContextMenu("Click")]
        public void Click()
        {
            OnClickedImplementation();
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
                OnClickedImplementation();
        }

        private void OnClicked(ClickEvent evt)
        {
            evt.PreventTrickleDown();

            if (!selectable)
                return;

            OnClickedImplementation();
        }

        private void OnClickedImplementation()
        {
            Clicked?.Invoke();
            clicked.Invoke();
        }

        private bool GetSelectable()
        {
            return selectable && !IsFading;
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
