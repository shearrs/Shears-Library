using System;
using Shears.Logging;
using Shears.Tweens;
using TMPro;
using UnityEngine;

namespace Shears.UI
{
    public partial class CanvasTooltip : UIElement, IColorTweenable
    {
        private static readonly TweenData FADE_DATA = new(
            0.1f,
            easingFunction: TweenEase.InOutQuad
        );

        [Header("Components")]
        [SerializeField, Required]
        private UIImage image;

        [SerializeField, RuntimeReadOnly]
        private UIElement hoverParent;

        [Header("Settings")]
        [
            SerializeField,
            Tooltip(
                "The amount of time the hover parent must be hovered before this tooltip appears."
            ),
            Min(0.0f)
        ]
        private float hoverTimeBeforeAppearing = 0.5f;

        [SerializeField]
        private bool usesUnscaledTime = true;

        [SerializeField]
        private bool staysOpenOnHover = true;

        [Header("Elements")]
        [SerializeField]
        private SerializableDictionary<string, UITextGUI> textElements = new();

        private readonly Timer appearTimer = new();
        private UIElement parent;

        public bool UsesUnscaledTime
        {
            get => usesUnscaledTime;
            set => usesUnscaledTime = value;
        }
        public bool IsHovered { get; private set; }

        public event Action BeforeFadeIn;

        protected override void Awake()
        {
            base.Awake();

            if (hoverParent != null)
                BindHover(hoverParent);
        }

        protected override void RegisterEvents()
        {
            RegisterEvent<HoverEnterEvent>(OnHoverEnter);
            RegisterEvent<HoverExitEvent>(OnHoverExit);
        }

        public void BindHover(UIElement parent)
        {
            if (parent == null)
            {
                Log("Cannot bind null parent!", SHLogLevels.Error);
                return;
            }

            if (this.parent != null)
                UnbindHover();

            parent.RegisterEvent<HoverEnterEvent>(OnParentHoverEnter);
            parent.RegisterEvent<HoverExitEvent>(OnParentHoverExit);

            this.parent = parent;
        }

        public void UnbindHover()
        {
            if (parent == null)
                return;

            parent.DeregisterEvent<HoverEnterEvent>(OnParentHoverEnter);
            parent.DeregisterEvent<HoverExitEvent>(OnParentHoverExit);
        }

        public void SetText(string key, string text)
        {
            if (!TryGetElement(key, out UITextGUI textElement))
                return;

            textElement.Text = text;
        }

        public bool TryGetElement<T>(string key, out T element)
            where T : Component
        {
            element = null;

            if (typeof(T) == typeof(UITextGUI))
            {
                if (textElements.TryGetValue(key, out var value))
                {
                    element = value as T;
                    return true;
                }

                return false;
            }

            Log(
                $"{nameof(CanvasTooltip)} does not support type {typeof(T).Name}!",
                SHLogLevels.Error
            );
            return false;
        }

        private void OnParentHoverEnter(HoverEnterEvent evt)
        {
            if (hoverTimeBeforeAppearing == 0.0f)
            {
                DisposeTweens();
                StoreTween(image.DoFadeTween(1.0f, FADE_DATA));
            }
            else if (appearTimer.IsDone)
            {
                appearTimer.Start(hoverTimeBeforeAppearing);
                appearTimer.Completed += OnAppearTimerCompleted;
            }
        }

        private void OnParentHoverExit(HoverExitEvent evt)
        {
            appearTimer.Stop();
            appearTimer.Completed -= OnAppearTimerCompleted;

            if (!IsHovered || !staysOpenOnHover)
            {
                DisposeTweens();
                StoreTween(image.DoFadeTween(0.0f, FADE_DATA));
            }
        }

        private void OnHoverEnter(HoverEnterEvent evt)
        {
            IsHovered = true;
        }

        private void OnHoverExit(HoverExitEvent evt)
        {
            IsHovered = false;

            if (evt.IsTricklingDown || evt.IsBubblingUp)
                return;

            appearTimer.Stop();
            appearTimer.Completed -= OnAppearTimerCompleted;

            DisposeTweens();
            StoreTween(image.DoFadeTween(0.0f, FADE_DATA));
        }

        private void OnAppearTimerCompleted()
        {
            BeforeFadeIn?.Invoke();
            appearTimer.Completed -= OnAppearTimerCompleted;

            DisposeTweens();
            StoreTween(image.DoFadeTween(1.0f, FADE_DATA));
        }
    }
}
