using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Shears.UI
{
    [RequireComponent(typeof(Selectable))]
    public class ManagedSelectable : MonoBehaviour
    {
        [Header("Initialization")]
        [SerializeField] private bool interactableOnEnable = true;

        [Header("Events")]
        [SerializeField] private UnityEvent onHover;
        [SerializeField] private UnityEvent onUnhover;
        [SerializeField] private UnityEvent onSelect;
        [SerializeField] private UnityEvent onUnselect;

        private Selectable selectable;
        private ManagedNavigation navigation;

        internal Selectable Selectable 
        { 
            get
            {
                if (selectable == null)
                    selectable = GetComponent<Selectable>();

                return selectable;
            }
        }

        public bool Interactable { get => Selectable.interactable; set => Selectable.interactable = value; }
        public ManagedNavigation Navigation
        {
            get
            {
                navigation ??= new(this);

                return navigation;
            }
        }

        protected virtual void OnEnable()
        {
            if (interactableOnEnable)
                Interactable = true;
            else
                Interactable = false;
        }

        internal virtual void Hover() => onHover?.Invoke();
        internal virtual void Unhover() => onUnhover?.Invoke();

        internal virtual void Select() => onSelect?.Invoke();
        internal virtual void Unselect() => onUnselect?.Invoke();
    }
}
