using Shears.Tweens;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    [RequireComponent(typeof(SpriteRenderer)), DisallowMultipleComponent]
    public class ManagedSpriteElement : ManagedWrapper<SpriteRenderer>, IColorTweenable
    {
        #region Flag Variables
        [SerializeField] private bool enableOnAwake = true;
        [SerializeField] private bool selectable = true;
        [SerializeField] private bool hoverable = true;

        public bool Selectable { get => selectable; set => selectable = value; }
        public bool Hoverable { get => hoverable; set => hoverable = value; }
        #endregion

        #region Event Variables
        [SerializeField] private UnityEvent onEnabled;
        [SerializeField] private UnityEvent onDisabled;
        [SerializeField] private UnityEvent onClickBegin;
        [SerializeField] private UnityEvent onClickEnd;
        [SerializeField] private UnityEvent onHoverBegin;
        [SerializeField] private UnityEvent onHoverEnd;
        [SerializeField] private UnityEvent onHoverBeginClicked;
        [SerializeField] private UnityEvent onHoverEndClicked;

        public event Action OnEnabled;
        public event Action OnDisabled;
        public event Action OnClickBegin;
        public event Action OnClickEnd;
        public event Action OnHoverBegin;
        public event Action OnHoverEnd;
        public event Action OnHoverBeginClicked;
        public event Action OnHoverEndClicked;
        #endregion

        #region Inspector Variables
#pragma warning disable CS0414
        [SerializeField] private bool spriteFoldout = false;
        [SerializeField] private bool flagsFoldout = false;
        [SerializeField] private bool eventsFoldout = false;
        [SerializeField] private bool activationFoldout = false;
        [SerializeField] private bool clickFoldout = false;
        [SerializeField] private bool hoverFoldout = false;
        [SerializeField] private bool hoverClickedFoldout = false;
#pragma warning restore CS0414
        #endregion

        [SerializeField] private Color baseColor = Color.white;

        private SpriteRenderer spriteRenderer;
        private bool isEnabled = false;
        private bool isHovered;
        private bool isClicked;

        private SpriteRenderer SpriteRenderer
        {
            get
            {
                if (spriteRenderer == null)
                    spriteRenderer = GetComponent<SpriteRenderer>();

                return spriteRenderer;
            }
        }

        public Color BaseColor { get => baseColor; set => baseColor = value; }
        public Color CurrentColor { get => SpriteRenderer.color; set => SpriteRenderer.color = value; }

        private void Awake()
        {
            ManagedSpriteEventSystem.CreateInstanceIfNoneExists();

            if (enableOnAwake)
                Enable();
            else
                Disable();
        }

        public void Enable()
        {
            if (isEnabled)
                return;

            SpriteRenderer.gameObject.SetActive(true);

            isEnabled = true;

            OnEnabled?.Invoke();
            onEnabled.Invoke();
        }

        public void Disable()
        {
            if (!isEnabled)
                return;

            SpriteRenderer.gameObject.SetActive(false);

            isEnabled = false;

            OnDisabled?.Invoke();
            onDisabled.Invoke();
        }

        #region Events
        public void BeginClick()
        {
            if (!Selectable)
                return;

            isClicked = true;

            OnClickBegin?.Invoke();
            onClickBegin.Invoke();
        }

        public void EndClick()
        {
            isClicked = false;

            if (!isHovered)
                return;

            OnClickEnd?.Invoke();
            onClickEnd.Invoke();
        }

        public void BeginHover()
        {
            if (!Hoverable)
                return;

            isHovered = true;

            if (isClicked)
            {
                OnHoverBeginClicked?.Invoke();
                onHoverBeginClicked.Invoke();
            }
            else
            {
                OnHoverBegin?.Invoke();
                onHoverBegin.Invoke();
            }
        }

        public void EndHover()
        {
            isHovered = false;

            if (isClicked)
            {
                OnHoverEndClicked?.Invoke();
                onHoverEndClicked.Invoke();
            }
            else
            {
                OnHoverEnd?.Invoke();
                onHoverEnd.Invoke();
            }
        }
        #endregion
    }
}
