using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    public class UIButton : UIElement
    {
        private const float COLOR_MOVE_TIME = 0.1f;

        [Header("UI Button")]
        [SerializeField]
        private ButtonGraphic graphic;

        [SerializeField]
        private bool selectable = true;

        [SerializeField]
        private bool clickOnMouseDown = false;

        [Header("Colors")]
        [SerializeField]
        private Color hoverColor = new(0.6f, 0.6f, 0.6f, 1.0f);

        [SerializeField]
        private Color pressColor = new(0.4f, 0.4f, 0.4f, 1.0f);

        [SerializeField]
        private Color notSelectableColor = new(0.15f, 0.15f, 0.15f);

        [Header("Events")]
        [SerializeField]
        private UnityEvent clicked;

        private readonly Timer colorTimer = new(COLOR_MOVE_TIME);
        private Color startColor;
        private Color targetColor;
        private bool isDragged;
        private bool isPressed;

        public bool IsHovered { get; private set; }
        public override Color BaseColor
        {
            get => graphic.BaseColor;
            set => graphic.BaseColor = value;
        }
        public override Color Modulate
        {
            get => graphic.Modulate;
            set => graphic.Modulate = value;
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
            [SerializeField, ShowIf(nameof(renderer), null)]
            private UIImage image;

            [SerializeField, ShowIf(nameof(image), null)]
            private Renderer renderer;

            private bool baseColorInitialized;
            private bool modulateInitialized;
            private Color baseColor;
            private Color modulate;
            private Color interactModulate = Color.white;

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
                            baseColor = renderer.material.color;
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

            public ButtonGraphic(UIImage image)
            {
                this.image = image;
            }

            public ButtonGraphic(Renderer renderer)
            {
                this.renderer = renderer;
            }

            private void UpdateGraphicColor()
            {
                if (image != null)
                {
                    image.BaseColor = BaseColor;
                    image.Modulate = InteractModulate * Modulate;
                }
                else if (renderer != null)
                    renderer.material.color = InteractModulate * Modulate * BaseColor;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (!selectable)
                graphic.InteractModulate = notSelectableColor.With(a: Modulate.a);
        }

        private void Update()
        {
            UpdateTargetColor();
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

        public void SetGraphic(UIImage image)
        {
            graphic = new(image);
        }

        public void SetGraphic(Renderer renderer)
        {
            graphic = new(renderer);
        }

        private void OnHoverEnter(HoverEnterEvent evt)
        {
            evt.PreventBubbleUp();

            IsHovered = true;
        }

        private void OnHoverExit(HoverExitEvent evt)
        {
            evt.PreventBubbleUp();

            Log("exit");
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
            Color newColor;

            if (!selectable)
                newColor = notSelectableColor;
            else
            {
                if (isDragged)
                    newColor = IsHovered ? pressColor : hoverColor;
                else if (isPressed)
                    newColor = pressColor;
                else if (IsHovered)
                    newColor = hoverColor;
                else if (IsFocused)
                    newColor = hoverColor;
                else
                    newColor = Color.white;
            }

            // If we are already this color, do nothing
            if (graphic.InteractModulate.CompareRGB(newColor))
                return;
            else if (targetColor != newColor || colorTimer.IsDone) // If this is a new color, or we aren't moving towards it, start moving toward it
            {
                colorTimer.Restart();
                targetColor = newColor;
                startColor = graphic.InteractModulate;
            }

            graphic.InteractModulate = Color
                .Lerp(startColor, targetColor, colorTimer.Percentage)
                .With(a: Modulate.a);
        }
    }
}
