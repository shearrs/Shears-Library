using System;
using System.Collections.Generic;
using Shears.Logging;
using Shears.Tweens;
using TMPro;
using UnityEngine;

namespace Shears.UI
{
    public partial class CanvasTooltip : UIElement
    {
        [Header("Components")]
        [SerializeField]
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
        private SerializableDictionary<string, TextMeshProUGUI> textElements = new();

        private readonly Timer appearTimer = new();
        private UIElement parent;

        public bool UsesUnscaledTime
        {
            get => usesUnscaledTime;
            set => usesUnscaledTime = value;
        }

        public event Action BeforeFadeIn;

        protected override void Awake()
        {
            base.Awake();

            if (hoverParent != null)
                BindHover(hoverParent);
        }

        protected override void RegisterEvents()
        {
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
            if (!TryGetElement(key, out TextMeshProUGUI textElement))
                return;

            textElement.text = text;
        }

        public bool TryGetElement<T>(string key, out T element)
            where T : Component
        {
            element = null;

            if (typeof(T) == typeof(TextMeshProUGUI))
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
                FadeIn();
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
                FadeOut();
        }

        private void OnHoverExit(HoverExitEvent evt)
        {
            if (evt.IsTricklingDown || staysOpenOnHover)
                return;

            appearTimer.Stop();
            appearTimer.Completed -= OnAppearTimerCompleted;

            FadeOut();
        }

        private void OnAppearTimerCompleted()
        {
            BeforeFadeIn?.Invoke();
            appearTimer.Completed -= OnAppearTimerCompleted;

            FadeIn();
        }
    }
}
