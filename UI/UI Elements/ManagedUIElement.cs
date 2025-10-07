using System;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    public class ManagedUIElement : MonoBehaviour
    {
        #region Flag Variables
        [SerializeField] private bool enableOnAwake = true;
        [SerializeField] private bool selectable = true;
        [SerializeField] private bool focusable = true;
        [SerializeField] private bool hoverable = true;

        public bool Selectable { get => selectable; set => selectable = value; }
        public bool Focusable { get => focusable; set => focusable = value; }
        public bool Hoverable { get => hoverable; set => hoverable = value; }
        #endregion

        #region Event Variables
        [SerializeField] private UnityEvent elementEnabled;
        [SerializeField] private UnityEvent elementDisabled;
        [SerializeField] private UnityEvent selectBegan;
        [SerializeField] private UnityEvent selectEnded;
        [SerializeField] private UnityEvent clickBegan;
        [SerializeField] private UnityEvent clickEnded;
        [SerializeField] private UnityEvent focusBegan;
        [SerializeField] private UnityEvent focusEnded;
        [SerializeField] private UnityEvent hoverBegan;
        [SerializeField] private UnityEvent hoverEnded;
        [SerializeField] private UnityEvent hoverBeganClicked;
        [SerializeField] private UnityEvent hoverEndedClicked;

        public event Action Enabled;
        public event Action Disabled;
        public event Action SelectBegan;
        public event Action SelectEnded;
        public event Action ClickBegan;
        public event Action ClickEnded;
        public event Action FocusBegan;
        public event Action FocusEnded;
        public event Action HoverBegan;
        public event Action HoverEnded;
        public event Action HoverBeganClicked;
        public event Action HoverEndedClicked;
        #endregion

        #region Inspector Variables
#if UNITY_EDITOR
#pragma warning disable CS0414
        [SerializeField] private bool flagsFoldout = false;
        [SerializeField] private bool eventsFoldout = false;
        [SerializeField] private bool activationFoldout = false;
        [SerializeField] private bool selectFoldout = false;
        [SerializeField] private bool clickFoldout = false;
        [SerializeField] private bool focusFoldout = false;
        [SerializeField] private bool hoverFoldout = false;
        [SerializeField] private bool hoverClickedFoldout = false;
        [SerializeField] private bool navigationFoldout = false;
#pragma warning restore CS0414
#endif
        #endregion

        [SerializeField] private ManagedUINavigation navigation;
        
        private bool isEnabled;
        private bool isHovered;
        private bool isClicked;

        public string ID { get; private set; }
        public RectTransform RectTransform { get; private set; }

        private void Awake()
        {
            ID = Guid.NewGuid().ToString();
            RectTransform = GetComponent<RectTransform>();

            navigation.Initialize(this);

            if (enableOnAwake)
                Enable();
        }

        private void OnDestroy()
        {
            if (ManagedUIEventSystem.IsInstanceActive())
            {
                ManagedUIEventSystem.DeregisterElement(this);
                navigation.Uninitialize();
            }
        }

        private void Update()
        {
            navigation.Update();
        }

        public void Enable()
        {
            if (isEnabled)
                return;

            isEnabled = true;

            ManagedUIEventSystem.RegisterElement(this);

            Enabled?.Invoke();
            elementEnabled.Invoke();
        }

        public void SetActiveAndEnable()
        {
            gameObject.SetActive(true);
            Enable();
        }

        public void Disable()
        {
            if (!isEnabled)
                return;

            isEnabled = false;

            ManagedUIEventSystem.DeregisterElement(this);

            Disabled?.Invoke();
            elementDisabled.Invoke();
        }

        public void SetInactiveAndDisable()
        {
            Disable();
            gameObject.SetActive(false);
        }

        public ManagedUIElement Navigate(ManagedUINavigation.Direction direction)
        {
            return navigation.GetElement(direction);
        }

        #region Events
        public void BeginSelect()
        {
            if (!Selectable)
                return;

            SelectBegan?.Invoke();
            selectBegan.Invoke();
        }

        public void EndSelect()
        {
            SelectEnded?.Invoke();
            selectEnded.Invoke();
        }

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

        public void BeginFocus()
        {
            if (!Focusable)
                return;

            FocusBegan?.Invoke();
            focusBegan.Invoke();
        }

        public void EndFocus()
        {
            FocusEnded?.Invoke();
            focusEnded.Invoke();
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
                HoverEndedClicked?.Invoke();
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
