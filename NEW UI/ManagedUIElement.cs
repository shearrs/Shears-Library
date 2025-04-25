using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    public class ManagedUIElement : MonoBehaviour
    {
        [Header("Flags")]
        [SerializeField] private bool selectable = true;
        [SerializeField] private bool focusable = true;
        [SerializeField] private bool hoverable = true;

        [Header("Select")]
        [SerializeField] private UnityEvent onSelectBegin;
        [SerializeField] private UnityEvent onSelectEnd;

        [Header("Focus")]
        [SerializeField] private UnityEvent onFocusBegin;
        [SerializeField] private UnityEvent onFocusEnd;

        [Header("Hover")]
        [SerializeField] private UnityEvent onHoverBegin;
        [SerializeField] private UnityEvent onHoverEnd;

        public string ID { get; private set; }
        public bool Selectable { get => selectable; set => selectable = value; }
        public bool Focusable { get => focusable; set => focusable = value; }
        public bool Hoverable { get => hoverable; set => hoverable = value; }

        public event Action OnSelectBegin;
        public event Action OnSelectEnd;
        public event Action OnFocusBegin;
        public event Action OnFocusEnd;
        public event Action OnHoverBegin;
        public event Action OnHoverEnd;

        private void Awake()
        {
            ID = Guid.NewGuid().ToString();

            ManagedEventSystem.RegisterElement(this);
            ManagedEventSystem.OnNavigationChanged += UpdateNavigation;
        }

        private void OnDestroy()
        {
            if (ManagedEventSystem.IsInstanceActive())
            {
                ManagedEventSystem.DeregisterElement(this);
                ManagedEventSystem.OnNavigationChanged -= UpdateNavigation;
            }
        }

        #region Navigation
        private void UpdateNavigation(IReadOnlyCollection<ManagedUIElement> elements)
        {
            // AUTOMATIC NAVIGATION
            // sort the elements into what direction they are
            //  - get the vector that points towards them
            //  - get the angle of the vector
            //  - use the angle to determine the quadrant (up, right, down, left)
            // choose the closest element in each quadrant
        }
        #endregion

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

            OnHoverBegin?.Invoke();
            onHoverBegin.Invoke();
        }

        public void EndHover()
        {
            OnHoverEnd?.Invoke();
            onHoverEnd.Invoke();
        }
        #endregion
    }
}
