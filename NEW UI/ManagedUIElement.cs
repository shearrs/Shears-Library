using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    public class ManagedUIElement : MonoBehaviour
    {
        #region Flag Variables
        [Header("Flags")]
        [SerializeField] private bool enableOnAwake = true;
        [SerializeField] private bool selectable = true;
        [SerializeField] private bool focusable = true;
        [SerializeField] private bool hoverable = true;

        public bool Selectable { get => selectable; set => selectable = value; }
        public bool Focusable { get => focusable; set => focusable = value; }
        public bool Hoverable { get => hoverable; set => hoverable = value; }
        #endregion

        #region Event Variables
        [Header("Activation")]
        [SerializeField] private UnityEvent onEnabled;
        [SerializeField] private UnityEvent onDisabled;

        [Header("Select")]
        [SerializeField] private UnityEvent onSelectBegin;
        [SerializeField] private UnityEvent onSelectEnd;

        [Header("Click")]
        [SerializeField] private UnityEvent onClickBegin;
        [SerializeField] private UnityEvent onClickEnd;

        [Header("Focus")]
        [SerializeField] private UnityEvent onFocusBegin;
        [SerializeField] private UnityEvent onFocusEnd;

        [Header("Hover")]
        [SerializeField] private UnityEvent onHoverBegin;
        [SerializeField] private UnityEvent onHoverEnd;
        [SerializeField] private UnityEvent onHoverBeginClicked;
        [SerializeField] private UnityEvent onHoverEndClicked;

        public event Action OnEnabled;
        public event Action OnDisabled;
        public event Action OnSelectBegin;
        public event Action OnSelectEnd;
        public event Action OnClickBegin;
        public event Action OnClickEnd;
        public event Action OnFocusBegin;
        public event Action OnFocusEnd;
        public event Action OnHoverBegin;
        public event Action OnHoverEnd;
        public event Action OnHoverBeginClicked;
        public event Action OnHoverEndClicked;
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
            if (ManagedEventSystem.IsInstanceActive())
            {
                ManagedEventSystem.DeregisterElement(this);
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

            ManagedEventSystem.RegisterElement(this);

            OnEnabled?.Invoke();
            onEnabled.Invoke();
        }

        public void Disable()
        {
            if (!isEnabled)
                return;

            isEnabled = false;

            ManagedEventSystem.DeregisterElement(this);

            OnDisabled?.Invoke();
            onDisabled.Invoke();
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

            OnSelectBegin?.Invoke();
            onSelectBegin.Invoke();
        }

        public void EndSelect()
        {
            OnSelectEnd?.Invoke();
            onSelectEnd.Invoke();
        }

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

        public void BeginFocus()
        {
            if (!Focusable)
                return;

            OnFocusBegin?.Invoke();
            onFocusBegin.Invoke();
        }

        public void EndFocus()
        {
            OnFocusEnd?.Invoke();
            onFocusEnd.Invoke();
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
