using System;
using System.Collections.Generic;
using Shears.Logging;
using Shears.Tweens;
using UnityEngine;

namespace Shears.UI
{
    [DisallowMultipleComponent]
    public class UIElement : SHMonoBehaviourLogger, IColorTweenable
    {
        #region Variables
        private readonly Dictionary<Type, object> registrations = new();
        private readonly Dictionary<IRef, object> refBindings = new();
        private readonly Dictionary<IRef, object> rawRefBindings = new();
        private readonly List<UIElement> childElements = new();
        private readonly TweenStorage tweenStorage = new();
        private UIElementCanvas canvas;
        private float dragBeginTime = 0.1f;

        protected IReadOnlyList<Tween> Tweens => tweenStorage.Tweens;
        public UIElementCanvas Canvas => canvas;
        public bool IsEnabled => isActiveAndEnabled;
        public bool IsHovered { get; internal set; }
        public bool IsFocused { get; internal set; }
        public float DragBeginTime
        {
            get => dragBeginTime;
            set => dragBeginTime = value;
        }
        public int SortOrder
        {
            get
            {
                if (Canvas == null)
                    return -1;

                return Canvas.GetSortOrder(this);
            }
        }
        public float Alpha
        {
            get => Modulate.a;
            set => SetAlpha(value);
        }
        public virtual Color BaseColor { get; set; }
        public virtual Color Modulate { get; set; }

        public event Action Disabled;
        #endregion

        #region Unity Methods
        protected virtual void Awake()
        {
            canvas = GetComponentInParent<UIElementCanvas>();

            UpdateChildList();

            RegisterEvents();

            BindRefs();
        }

        protected virtual void OnDisable()
        {
            DisposeTweens();

            Disabled?.Invoke();
        }

        protected virtual void OnDestroy()
        {
            Unbind();
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            Invoke(nameof(SetLayer), 0f);
        }

        private void OnTransformParentChanged()
        {
            canvas = GetComponentInParent<UIElementCanvas>();
        }

        private void OnTransformChildrenChanged()
        {
            UpdateChildList();
        }
        #endregion

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            bool wasEnabled = IsEnabled;

            gameObject.SetActive(false);

            if (wasEnabled)
                Disabled?.Invoke();
        }

        protected virtual void BindRefs() { }

        public virtual void SetAlpha(float alpha)
        {
            Modulate = Modulate.With(a: alpha);

            foreach (var child in childElements)
                child.SetAlpha(alpha);
        }

        #region Event Registration
        public void RegisterEvent<EventType>(Action<EventType> callback)
            where EventType : UIEvent
        {
            var eventType = typeof(EventType);

            if (!registrations.TryGetValue(eventType, out var list))
            {
                list = new List<IEventRegistration<EventType>>();
                registrations[eventType] = list;
            }

            ((List<IEventRegistration<EventType>>)list).Add(
                new EventRegistration<EventType>(callback)
            );
        }

        public void DeregisterEvent<EventType>(Action<EventType> callback)
            where EventType : UIEvent
        {
            var eventType = typeof(EventType);

            if (!registrations.TryGetValue(eventType, out var list))
                return;

            ((List<IEventRegistration<EventType>>)list).Remove(
                new EventRegistration<EventType>(callback)
            );
        }

        internal void InvokeEvent<EventType>(EventType evt)
            where EventType : UIEvent
        {
            if (registrations.TryGetValue(typeof(EventType), out var list))
            {
                foreach (var registration in (List<IEventRegistration<EventType>>)list)
                    registration.Invoke(evt);
            }

            if (!evt.IsBubblingUp && evt.TrickleDown)
            {
                evt.IsTricklingDown = true;

                foreach (var child in childElements)
                {
                    if (child == this)
                        continue;

                    child.InvokeEvent(evt);
                }

                evt.IsTricklingDown = false;
            }

            if (!evt.IsTricklingDown && evt.BubbleUp)
            {
                evt.IsBubblingUp = true;

                var parent = transform.parent;

                if (parent == null)
                    return;

                var parentElement = parent.GetComponentInParent<UIElement>();

                if (parentElement != null)
                    parentElement.InvokeEvent(evt);

                evt.IsBubblingUp = false;
            }
        }
        #endregion

        #region Tweens
        protected Tween GetFirstValidTween() => tweenStorage.GetFirstValid();

        protected Tween StoreTween(Tween tween) => tweenStorage.Store(tween);

        protected void DisposeTweens() => tweenStorage.Dispose();

        public Tween DoColorTween(
            Color targetColor,
            ITweenData data = null,
            bool affectsAlpha = false
        ) => ((IColorTweenable)this).DoColorTween(targetColor, data, affectsAlpha);

        public Tween GetColorTween(
            Color targetColor,
            ITweenData data = null,
            bool affectsAlpha = false
        ) => ((IColorTweenable)this).GetColorTween(targetColor, data, affectsAlpha);

        public Tween DoFadeTween(float alpha, ITweenData data = null) =>
            ((IAlphaTweenable)this).DoFadeTween(alpha, data);

        public Tween GetFadeTween(float alpha, ITweenData data = null) =>
            ((IAlphaTweenable)this).GetFadeTween(alpha, data);
        #endregion

        #region Children
        internal bool IsChildOfCanvas() => canvas != null;

        internal UIElement GetDeepestChild()
        {
            GetDeepestChildRecursive(0, out var child);

            return child;
        }

        private int GetDeepestChildRecursive(int depth, out UIElement deepestChild)
        {
            int deepestDepth = depth;
            deepestChild = this;

            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);

                if (!child.TryGetComponent(out UIElement element))
                    continue;

                int currentDepth = element.GetDeepestChildRecursive(
                    depth + 1,
                    out var currentChild
                );

                if (currentDepth > deepestDepth)
                    deepestChild = currentChild;
            }

            return deepestDepth;
        }

        private void UpdateChildList()
        {
            GetComponentsInChildren(childElements);
            childElements.Remove(this);
        }
        #endregion

        public void Focus() => UIElementEventSystem.Focus(this);

        public void Blur() => UIElementEventSystem.Focus(null);

        #region Binding Events
        protected void Bind<T>(IReadOnlyRef<T> refVar, RefChangeEvent<T> action)
        {
            if (refBindings.ContainsKey(refVar))
            {
                Log($"{nameof(UIElement)} already has binding for ${refVar}!", SHLogLevels.Warning);
                return;
            }

            refVar.Bind(action);
        }

        protected void BindRaw<T>(IReadOnlyRef<T> refVar, Action<T> action)
        {
            if (rawRefBindings.ContainsKey(refVar))
            {
                Log(
                    $"{nameof(UIElement)} already has raw binding for ${refVar}!",
                    SHLogLevels.Warning
                );
                return;
            }

            refVar.BindRaw(action);
        }

        protected void Unbind()
        {
            foreach (var (refVar, action) in refBindings)
                refVar.Unbind(action);

            foreach (var (refVar, action) in rawRefBindings)
                refVar.Unbind(action);

            refBindings.Clear();
        }

        protected void Unbind<T>(IReadOnlyRef<T> refVar, RefChangeEvent<T> action)
        {
            refVar.Changed -= action;

            refBindings.Remove(refVar);
        }

        protected void UnbindRaw<T>(IReadOnlyRef<T> refVar, Action<T> action)
        {
            refVar.ChangedRaw -= action;

            rawRefBindings.Remove(refVar);
        }

        protected virtual void RegisterEvents() { }
        #endregion

        private void SetLayer()
        {
            gameObject.layer = LayerMask.NameToLayer("UI");
        }
    }
}
