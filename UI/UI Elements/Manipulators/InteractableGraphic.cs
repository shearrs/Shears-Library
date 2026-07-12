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
        public static readonly Color DefaultNotSelectableColor = Color.white;

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
        [SerializeField, ReadOnly, ShowIf("!renderer", compareValue: null)]
        private Color baseMaterialColor = Color.white;

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
        private bool baseMaterialColorInitialized = false;
        private bool spriteInitialized = false;

        public Color BaseMaterialColor
        {
            get
            {
                if (!baseMaterialColorInitialized)
                    InitializeBaseMaterialColor();

                return baseMaterialColor;
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
            baseMaterialColorInitialized = false;
            spriteInitialized = false;
        }

        private void InitializeBaseMaterialColor()
        {
            baseMaterialColor = renderer.material.color;
            baseMaterialColorInitialized = true;
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
            if (interactModulate != Color.clear)
                return;

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
            if (image != null)
                image.Modulate = InteractModulate;
            else if (renderer != null)
            {
                if (!spriteInitialized)
                    InitializeSprite();

                if (uiSprite != null)
                    uiSprite.Modulate = InteractModulate;
                else
                    renderer.material.color = InteractModulate * BaseMaterialColor;
            }
            else if (text != null)
                text.Modulate = InteractModulate;
            else if (textGUI != null)
                textGUI.Modulate = InteractModulate;
        }
        #endregion
    }
}
