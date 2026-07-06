using Shears.Tweens;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;

namespace Shears.UI
{
    [System.Serializable, MovedFrom(true, "Shears.UI", "Assembly-CSharp", "UIButton.ButtonGraphic")]
    public class InteractableGraphic
    {
        #region Variables
        private static readonly TweenData COLOR_TWEEN = new(0.1f);
        public static readonly Color DefaultHoverColor = new(0.6f, 0.6f, 0.6f, 1.0f);
        public static readonly Color DefaultPressColor = new(0.4f, 0.4f, 0.4f, 1.0f);
        public static readonly Color DefaultNotSelectableColor = new(0.15f, 0.15f, 0.15f);

        public enum InteractColor
        {
            None,
            Hover,
            Press,
            NotSelectable,
        }

        [
            SerializeField,
            Required(nameof(renderer), nameof(text), nameof(textGUI)),
            ShowIf(compareValue: null, nameof(renderer), nameof(text), nameof(textGUI))
        ]
        private UIImage image;

        [
            SerializeField,
            Required(nameof(image), nameof(text), nameof(textGUI)),
            ShowIf(compareValue: null, nameof(image), nameof(text), nameof(textGUI))
        ]
        private Renderer renderer;

        [
            SerializeField,
            Required(nameof(image), nameof(renderer), nameof(textGUI)),
            ShowIf(compareValue: null, nameof(image), nameof(renderer), nameof(textGUI))
        ]
        private UIText text;

        [
            SerializeField,
            Required(nameof(image), nameof(renderer), nameof(text)),
            ShowIf(compareValue: null, nameof(image), nameof(renderer), nameof(text))
        ]
        private UITextGUI textGUI;

        [Header("State Colors")]
        [SerializeField, ReadOnly]
        private Color baseColor = Color.white;

        [SerializeField, ReadOnly]
        private Color modulate = Color.white;

        [SerializeField, ReadOnly]
        private Color interactModulate = Color.white;

        [Header("Colors")]
        [SerializeField]
        private Color hoverColor = DefaultHoverColor;

        [SerializeField]
        private Color pressColor = DefaultPressColor;

        [SerializeField]
        private Color notSelectableColor = DefaultNotSelectableColor;

        private UISprite uiSprite;
        private Tween colorTween;
        private bool spriteInitialized = false;
        private bool baseColorInitialized;
        private bool modulateInitialized;

        public Color BaseColor
        {
            get
            {
                if (!baseColorInitialized)
                    InitializeBaseColor();

                return baseColor;
            }
            set
            {
                baseColorInitialized = true;
                baseColor = value;

                UpdateGraphicColor();
            }
        }
        public Color Modulate
        {
            get
            {
                if (!modulateInitialized)
                    InitializeModulate();

                return modulate;
            }
            set
            {
                modulateInitialized = true;
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
        #endregion

        #region Initialization
        public InteractableGraphic()
        {
            interactModulate = Color.white;
        }

        public InteractableGraphic(UIImage image)
        {
            this.image = image;
        }

        public InteractableGraphic(Renderer renderer)
        {
            this.renderer = renderer;
        }

        public InteractableGraphic(UIText text)
        {
            this.text = text;

            hoverColor = Color.white;
            pressColor = Color.white;
            notSelectableColor = Color.white;
        }

        public InteractableGraphic(UITextGUI text)
        {
            textGUI = text;

            hoverColor = Color.white;
            pressColor = Color.white;
            notSelectableColor = Color.white;
        }

        ~InteractableGraphic()
        {
            colorTween.Dispose();
        }

        public void Reset()
        {
            baseColorInitialized = false;
            modulateInitialized = false;
        }

        public void SetManaged(bool value)
        {
            if (renderer != null && !spriteInitialized)
                InitializeSprite();

            if (image != null)
            {
                image.IsAlphaManaged = value;
                image.IsBaseColorManaged = value;
                image.IsModulateManaged = value;
            }
            else if (uiSprite != null)
            {
                uiSprite.IsAlphaManaged = value;
                uiSprite.IsBaseColorManaged = value;
                uiSprite.IsModulateManaged = value;
            }
            else if (text != null)
            {
                text.IsAlphaManaged = value;
                text.IsBaseColorManaged = value;
                text.IsModulateManaged = value;
            }
            else if (textGUI != null)
            {
                textGUI.IsAlphaManaged = value;
                textGUI.IsBaseColorManaged = value;
                textGUI.IsModulateManaged = value;
            }
        }

        private void InitializeBaseColor()
        {
            baseColorInitialized = true;

            if (image != null)
                baseColor = image.BaseColor;
            else if (renderer != null)
            {
                if (!spriteInitialized)
                    InitializeSprite();

                if (uiSprite != null)
                    baseColor = uiSprite.BaseColor;
                else if (renderer is SpriteRenderer sprite)
                    baseColor = sprite.color;
                else
                    baseColor = renderer.material.color;
            }
            else if (text != null)
                baseColor = text.BaseColor;
            else if (textGUI != null)
                baseColor = textGUI.BaseColor;
        }

        private void InitializeModulate()
        {
            modulateInitialized = true;

            if (image != null)
                modulate = image.Modulate;
            else if (renderer != null)
            {
                if (!spriteInitialized)
                    InitializeSprite();

                if (uiSprite != null)
                    modulate = uiSprite.Modulate;
                else
                    modulate = Color.white;
            }
            else if (text != null)
                modulate = text.Modulate;
            else if (textGUI != null)
                modulate = textGUI.Modulate;
        }

        private void InitializeSprite()
        {
            renderer.TryGetComponent(out uiSprite);
            spriteInitialized = true;
        }
        #endregion

        #region Colors
        public void ValidateColors()
        {
            if (baseColor != Color.clear)
                return;

            baseColor = Color.white;
            modulate = Color.white;
            interactModulate = Color.white;
            hoverColor = DefaultHoverColor;
            pressColor = DefaultPressColor;
            notSelectableColor = DefaultNotSelectableColor;

            Reset();
        }

        public bool IsInteractColor(InteractColor color) =>
            InteractModulate == GetColorForInteract(color);

        public void MoveTowardsColor(InteractColor color)
        {
            colorTween.Dispose();

            TargetColor = color;
            var startColor = InteractModulate;
            var realColor = GetColorForInteract(color);
            Object lifetime = image != null ? image : renderer;

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
            if (!baseColorInitialized)
                InitializeBaseColor();

            if (!modulateInitialized)
                InitializeModulate();

            if (image != null)
            {
                image.SetBaseColorManaged(BaseColor);
                image.SetModulateManaged(InteractModulate * Modulate);
            }
            else if (renderer != null)
            {
                if (!spriteInitialized)
                    InitializeSprite();

                if (uiSprite != null)
                {
                    uiSprite.SetBaseColorManaged(BaseColor);
                    uiSprite.SetModulateManaged(InteractModulate * Modulate);
                }
                else if (renderer is SpriteRenderer sprite)
                    sprite.color = InteractModulate * Modulate * BaseColor;
                else
                    renderer.material.color = InteractModulate * Modulate * BaseColor;
            }
            else if (text != null)
            {
                text.SetBaseColorManaged(BaseColor);
                text.SetModulateManaged(InteractModulate * Modulate);
            }
            else if (textGUI != null)
            {
                textGUI.SetBaseColorManaged(BaseColor);
                textGUI.SetModulateManaged(InteractModulate * Modulate);
            }
        }
        #endregion
    }
}
