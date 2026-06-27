using System;
using System.Collections.Generic;
using Shears.Logging;
using Shears.Tweens;
using UnityEngine;

namespace Shears.UI
{
    public class UIElement : SHMonoBehaviourLogger
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

        public event Action Disabled;
        #endregion

        #region Unity Methods
        protected virtual void Awake()
        {
            canvas = GetComponentInParent<UIElementCanvas>();

            UpdateChildLists();

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
            UpdateChildLists();
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

        public void SetAlpha(float alpha)
        {
            foreach (var child in childElements)
                child.SetAlpha(alpha);
        }

        public Tween DoFadeTween(float alpha, ITweenData data)
        {
            var tween = TweenManager.CreateTween(
                (t) =>
                {
                    foreach (var child in childElements)
                        child.TweenFadeUpdate(alpha, data);
                }
            );

            return tween;
        }

        protected virtual void TweenFadeUpdate(float alpha, ITweenData data) { }

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

        private void UpdateChildLists() { }
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
