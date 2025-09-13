using Shears.Tweens;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    [RequireComponent(typeof(SpriteRenderer)), DisallowMultipleComponent]
    public class ManagedSpriteElement : ManagedWrapper<SpriteRenderer>, IColorTweenable
    {
        private enum AwakeBehaviour { None, Enable, Disable }

        #region Flag Variables
        [SerializeField] private AwakeBehaviour awakeBehaviour;
        [SerializeField, ReadOnly] private bool isEnabled = false;
        [SerializeField] private bool selectable = true;
        [SerializeField] private bool hoverable = true;

        public bool Selectable { get => selectable; set => selectable = value; }
        public bool Hoverable { get => hoverable; set => hoverable = value; }
        #endregion

        #region Event Variables
        [SerializeField] private UnityEvent enabledEvent;
        [SerializeField] private UnityEvent disabledEvent;
        [SerializeField] private UnityEvent clickBegan;
        [SerializeField] private UnityEvent clickEnded;
        [SerializeField] private UnityEvent hoverBegan;
        [SerializeField] private UnityEvent hoverEnded;
        [SerializeField] private UnityEvent hoverBeganClicked;
        [SerializeField] private UnityEvent hoverEndedClicked;

        public event Action Enabled;
        public event Action Disabled;
        public event Action ClickBegan;
        public event Action ClickEnded;
        public event Action HoverBegan;
        public event Action HoverEnded;
        public event Action HoverBeganClicked;
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

        public Sprite Sprite { get => SpriteRenderer.sprite; set => SpriteRenderer.sprite = value; }
        public Color BaseColor { get => baseColor; set => baseColor = value; }
        public Color Modulate { get => SpriteRenderer.color; set => SpriteRenderer.color = value; }
        public int SortingOrder { get => SpriteRenderer.sortingOrder; set => SpriteRenderer.sortingOrder = value; }

        private void Awake()
        {
            ManagedSpriteEventSystem.CreateInstanceIfNoneExists();

            switch (awakeBehaviour)
            {
                case AwakeBehaviour.Enable:
                    Enable();
                    break;
                case AwakeBehaviour.Disable:
                    isEnabled = true;
                    Disable();
                    break;
                case AwakeBehaviour.None:
                    break;
            }
        }

        public void Enable()
        {
            if (isEnabled)
                return;

            SpriteRenderer.gameObject.SetActive(true);

            isEnabled = true;

            Enabled?.Invoke();
            enabledEvent.Invoke();
        }

        public void Disable()
        {
            if (!isEnabled)
                return;

            SpriteRenderer.gameObject.SetActive(false);

            isEnabled = false;

            Disabled?.Invoke();
            disabledEvent.Invoke();
        }

        #region Events
        public void BeginClick()
        {
            if (!Selectable)
                return;

            isClicked = true;

            ClickBegan?.Invoke();
            clickBegan.Invoke();
        }

        public void EndClick()
        {
            isClicked = false;

            if (!isHovered)
                return;

            ClickEnded?.Invoke();
            clickEnded.Invoke();
        }

        public void BeginHover()
        {
            if (!Hoverable)
                return;

            isHovered = true;

            if (isClicked)
            {
                HoverBeganClicked?.Invoke();
                hoverBeganClicked.Invoke();
            }
            else
            {
                HoverBegan?.Invoke();
                hoverBegan.Invoke();
            }
        }

        public void EndHover()
        {
            isHovered = false;

            if (isClicked)
            {
                OnHoverEndClicked?.Invoke();
                hoverEndedClicked.Invoke();
            }
            else
            {
                HoverEnded?.Invoke();
                hoverEnded.Invoke();
            }
        }
        #endregion
    }
}
