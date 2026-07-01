using System;
using System.Collections.Generic;
using Shears.Tweens;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    public class UIButton : UIElement
    {
        private static readonly TweenData COLOR_TWEEN = new(0.1f);
        private static readonly Color HOVER_COLOR = new(0.6f, 0.6f, 0.6f, 1.0f);
        private static readonly Color PRESS_COLOR = new(0.4f, 0.4f, 0.4f, 1.0f);
        private static readonly Color NOT_SELECTABLE_COLOR = new(0.15f, 0.15f, 0.15f);

        private enum InteractColor
        {
            None,
            Hover,
            Press,
            NotSelectable,
        }

        [Header("UI Button")]
        [SerializeField, Required(targetCollectionSize: 1)]
        private List<ButtonGraphic> graphics = new();

        [SerializeField]
        private bool selectable = true;

        [SerializeField]
        private bool clickOnMouseDown = false;

        [Header("Events")]
        [SerializeField]
        private UnityEvent clicked;

        private Color baseColor = Color.white;
        private Color modulate = Color.white;
        private bool isDragged;
        private bool isPressed;

        public bool IsHovered { get; private set; }
        public override Color BaseColor
        {
            get => baseColor;
            set => SetBaseColor(value);
        }
        public override Color Modulate
        {
            get => modulate;
            set => SetModulate(value);
        }
        public bool Selectable
        {
            get => selectable;
            set => SetSelectable(value);
        }

        public event Action Clicked;

        [Serializable]
        private class ButtonGraphic
        {
            [
                SerializeField,
                Required(nameof(renderer)),
                ShowIf(nameof(renderer), compareValue: null)
            ]
            private UIImage image;

            [SerializeField, Required(nameof(image)), ShowIf(nameof(image), compareValue: null)]
            private Renderer renderer;

            [Header("State Colors")]
            [SerializeField, ReadOnly]
            private Color baseColor = Color.white;

            [SerializeField, ReadOnly]
            private Color modulate = Color.white;

            [SerializeField, ReadOnly]
            private Color interactModulate = Color.white;

            [Header("Colors")]
            [SerializeField]
            private Color hoverColor = HOVER_COLOR;

            [SerializeField]
            private Color pressColor = PRESS_COLOR;

            [SerializeField]
            private Color notSelectableColor = NOT_SELECTABLE_COLOR;

            private Tween colorTween;
            private bool baseColorInitialized;
            private bool modulateInitialized;

            public Color BaseColor
            {
                get
                {
                    if (image == null && renderer == null)
                        return baseColor;

                    if (!baseColorInitialized)
                    {
                        baseColorInitialized = true;

                        if (image != null)
                            baseColor = image.BaseColor;
                        else if (renderer != null)
                        {
                            if (renderer is SpriteRenderer sprite)
                                baseColor = sprite.color;
                            else
                                baseColor = renderer.material.color;
                        }
                    }

                    return baseColor;
                }
                set
                {
                    baseColor = value;

                    if (!modulateInitialized)
                    {
                        modulateInitialized = true;

                        if (image != null)
                            modulate = image.Modulate;
                        else
                            modulate = Color.white;
                    }

                    UpdateGraphicColor();
                }
            }
            public Color Modulate
            {
                get
                {
                    if (image == null && renderer == null)
                        return modulate;

                    if (!modulateInitialized)
                    {
                        modulateInitialized = true;

                        if (image != null)
                            modulate = image.Modulate;
                        else
                            modulate = Color.white;
                    }

                    return modulate;
                }
                set
                {
                    modulate = value;

                    UpdateGraphicColor();
                }
            }
            public Color InteractModulate
            {
                get => interactModulate;
                set
                {
                    interactModulate = value;

                    UpdateGraphicColor();
                }
            }
            public Color HoverColor
            {
                get => hoverColor;
                set => hoverColor = value;
            }
            public Color PressColor
            {
                get => pressColor;
                set => pressColor = value;
            }
            public Color NotSelectableColor
            {
                get => notSelectableColor;
                set => notSelectableColor = value;
            }
            public InteractColor TargetColor { get; private set; }
            public bool IsMovingTowardsColor => colorTween.IsPlaying;

            public ButtonGraphic()
            {
                interactModulate = Color.white;
            }

            public ButtonGraphic(UIImage image)
            {
                this.image = image;
            }

            public ButtonGraphic(Renderer renderer)
            {
                this.renderer = renderer;
            }

            ~ButtonGraphic()
            {
                colorTween.Dispose();
            }

            public void Reset()
            {
                baseColorInitialized = false;
                modulateInitialized = false;
            }

            public bool IsInteractColor(InteractColor color) =>
                InteractModulate == GetColorForInteract(color);

            public void MoveTowardsColor(InteractColor color)
            {
                colorTween.Dispose();

                TargetColor = color;
                var startColor = InteractModulate;
                var realColor = GetColorForInteract(color);
                UnityEngine.Object lifetime = image != null ? image : renderer;

                colorTween = TweenManager
                    .CreateTween(
                        t =>
                        {
                            InteractModulate = Color.LerpUnclamped(startColor, realColor, t);
                        },
                        COLOR_TWEEN
                    )
                    .WithLifetime(lifetime);

                colorTween.Play();
            }

            private Color GetColorForInteract(InteractColor color)
            {
                return color switch
                {
                    InteractColor.None => Color.white,
                    InteractColor.Hover => hoverColor,
                    InteractColor.Press => pressColor,
                    InteractColor.NotSelectable => notSelectableColor,
                    _ => Color.clear,
                };
            }

            private void UpdateGraphicColor()
            {
                if (image != null)
                {
                    image.BaseColor = BaseColor;
                    image.Modulate = InteractModulate * Modulate;
                }
                else if (renderer != null)
                {
                    if (renderer is SpriteRenderer sprite)
                        sprite.color = InteractModulate * Modulate * BaseColor;
                    else
                        renderer.material.color = InteractModulate * Modulate * BaseColor;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (!selectable)
            {
                foreach (var graphic in graphics)
                    graphic.InteractModulate = graphic.NotSelectableColor.With(
                        a: graphic.Modulate.a
                    );
            }
        }

        private void Update()
        {
            UpdateTargetColor();
        }

        private void OnValidate()
        {
            foreach (var graphic in graphics)
            {
                if (graphic.HoverColor == Color.clear)
                {
                    graphic.BaseColor = Color.white;
                    graphic.Modulate = Color.white;
                    graphic.InteractModulate = Color.white;
                    graphic.HoverColor = HOVER_COLOR;
                    graphic.PressColor = PRESS_COLOR;
                    graphic.NotSelectableColor = NOT_SELECTABLE_COLOR;

                    graphic.Reset();
                }
            }
        }

        [ContextMenu("Click")]
        public void Click()
        {
            OnClickedImplementation();
        }

        protected override void RegisterEvents()
        {
            RegisterEvent<HoverEnterEvent>(OnHoverEnter);
            RegisterEvent<HoverExitEvent>(OnHoverExit);
            RegisterEvent<PointerDownEvent>(OnPointerDown);
            RegisterEvent<PointerUpEvent>(OnPointerUp);
            RegisterEvent<ClickEvent>(OnClicked);
            RegisterEvent<DragBeginEvent>(OnDragBegin);
            RegisterEvent<DragEndEvent>(OnDragEnd);
        }

        public void AddGraphic(UIImage image)
        {
            graphics.Add(new(image));
        }

        public void AddGraphic(Renderer renderer)
        {
            graphics.Add(new(renderer));
        }

        private void OnHoverEnter(HoverEnterEvent evt)
        {
            evt.PreventBubbleUp();

            IsHovered = true;
        }

        private void OnHoverExit(HoverExitEvent evt)
        {
            evt.PreventBubbleUp();

            IsHovered = false;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            evt.PreventTrickleDown();

            if (!selectable)
                return;

            isPressed = true;

            if (clickOnMouseDown)
                OnClickedImplementation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            isPressed = false;
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

        private void OnDragBegin(DragBeginEvent evt)
        {
            isDragged = true;
        }

        private void OnDragEnd(DragEndEvent evt)
        {
            isDragged = false;
        }

        private void SetSelectable(bool value)
        {
            selectable = value;

            if (!value)
            {
                isPressed = false;
                isDragged = false;
            }
        }

        private void UpdateTargetColor()
        {
            InteractColor newColor;

            if (!selectable)
                newColor = InteractColor.NotSelectable;
            else
            {
                if (isDragged)
                    newColor = IsHovered ? InteractColor.Press : InteractColor.Hover;
                else if (isPressed)
                    newColor = InteractColor.Press;
                else if (IsHovered)
                    newColor = InteractColor.Hover;
                else if (IsFocused)
                    newColor = InteractColor.Hover;
                else
                    newColor = InteractColor.None;
            }

            foreach (var graphic in graphics)
            {
                if (graphic.IsInteractColor(newColor))
                    continue;
                else if (graphic.TargetColor == newColor && graphic.IsMovingTowardsColor)
                    continue;

                graphic.MoveTowardsColor(newColor);
            }
        }

        private void SetBaseColor(Color value)
        {
            baseColor = value;

            foreach (var graphic in graphics)
                graphic.BaseColor = baseColor;
        }

        private void SetModulate(Color value)
        {
            modulate = value;

            foreach (var graphic in graphics)
                graphic.Modulate = modulate;
        }
    }
}
