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

        [Header("Focus")]
        [SerializeField] private UnityEvent onFocusBegin;
        [SerializeField] private UnityEvent onFocusEnd;

        [Header("Hover")]
        [SerializeField] private UnityEvent onHoverBegin;
        [SerializeField] private UnityEvent onHoverEnd;

        public event Action OnEnabled;
        public event Action OnDisabled;
        public event Action OnSelectBegin;
        public event Action OnSelectEnd;
        public event Action OnFocusBegin;
        public event Action OnFocusEnd;
        public event Action OnHoverBegin;
        public event Action OnHoverEnd;
        #endregion

        private bool isEnabled;
        private ManagedUINavigation navigation;

        public string ID { get; private set; }

        private void Awake()
        {
            ID = Guid.NewGuid().ToString();

            ManagedEventSystem.OnNavigationChanged += UpdateNavigation;

            Enable();
        }

        private void OnDestroy()
        {
            if (ManagedEventSystem.IsInstanceActive())
            {
                ManagedEventSystem.DeregisterElement(this);
                ManagedEventSystem.OnNavigationChanged -= UpdateNavigation;
            }
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

        #region Navigation
        private void UpdateNavigation(IReadOnlyCollection<ManagedUIElement> elements)
        {
            var newNavigation = new ManagedUINavigation();
            Vector3 position = transform.position;

            foreach (var element in elements)
            {
                if (element == this)
                    continue;

                var direction = GetDirectionToElement(element);
                var currentElement = navigation.GetElement(direction);

                if (currentElement == null) // if we don't already have an element in this direction
                    newNavigation.SetElement(element, direction);
                else if (Vector2.Distance(position, element.transform.position) < Vector2.Distance(position, currentElement.transform.position)) // if this element is closer
                    newNavigation.SetElement(element, direction);
            }

            navigation = newNavigation;
        }

        private ManagedUINavigation.Direction GetDirectionToElement(ManagedUIElement element)
        {
            Vector2 direction = (element.transform.position - transform.position).normalized;
            Vector2 right = Vector2.right;

            float angle = Vector2.SignedAngle(right, direction);

            Debug.Log("angle: " + angle, this);

            if (angle <= 135 && angle > 45)
                return ManagedUINavigation.Direction.Up;
            else if (angle <= 45 && angle > -45)
                return ManagedUINavigation.Direction.Right;
            else if (angle <= -45 && angle > -135)
                return ManagedUINavigation.Direction.Down;
            else
                return ManagedUINavigation.Direction.Left;
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
