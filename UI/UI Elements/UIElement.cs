using System;
using System.Collections.Generic;
using Shears.Logging;
using Shears.Tweens;
using TMPro;
using TreeEditor;
using UnityEngine;
using UnityEngine.UI;
using static Codice.CM.Common.CmCallContext;

namespace Shears.UI
{
    public class UIElement : SHMonoBehaviourLogger
    {
        private static readonly TweenData FadeTweenData = new(0.1f, unscaledTime: true);

        [Header("UI Element")]
        [SerializeField, RuntimeReadOnly]
        private GameObject graphicsContainer;

        private readonly Dictionary<Type, object> registrations = new();
        private readonly Dictionary<IRef, object> refBindings = new();
        private readonly Dictionary<IRef, object> rawRefBindings = new();
        private readonly List<UIElement> childElements = new();
        private readonly List<TextMeshPro> textChildren = new();
        private readonly List<SpriteRenderer> spriteChildren = new();
        private readonly TweenStorage tweenStorage = new();
        private SpriteRenderer spriteRenderer;
        private bool isEnabled = true;
        private bool isFadingIn = false;
        private bool isFadingOut = false;
        private float dragBeginTime = 0.1f;

        protected IReadOnlyList<Tween> Tweens => tweenStorage.Tweens;
        public GameObject GraphicsContainer
        {
            get
            {
                if (graphicsContainer == null)
                    graphicsContainer = gameObject;

                return graphicsContainer;
            }
            set => graphicsContainer = value;
        }
        public bool IsEnabled => isEnabled;
        public bool IsHovered { get; internal set; }
        public float DragBeginTime
        {
            get => dragBeginTime;
            set => dragBeginTime = value;
        }

        public event Action Disabled;
        public event Action FadeInBegan;
        public event Action FadeInCompleted;
        public event Action FadeOutBegan;
        public event Action FadeOutCompleted;

        protected virtual void Awake()
        {
            TryGetComponent(out spriteRenderer);
            UpdateChildLists(transform);

            if (GraphicsContainer.activeInHierarchy)
                Enable();
            else
                Disable();

            RegisterEvents();

            BindRefs();
        }

        protected virtual void OnDisable()
        {
            Disabled?.Invoke();
        }

        protected virtual void OnDestroy()
        {
            Unbind();
        }

        private void OnValidate()
        {
            Invoke(nameof(SetLayer), 0f);
        }

        private void OnTransformChildrenChanged()
        {
            UpdateChildLists(transform);
        }

        private void UpdateChildLists(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                if (child.TryGetComponent(out UIElement element))
                {
                    childElements.Add(element);
                    continue;
                }

                if (child.TryGetComponent(out TextMeshPro text))
                    textChildren.Add(text);

                if (child.TryGetComponent(out SpriteRenderer sprite))
                    spriteChildren.Add(sprite);

                UpdateChildLists(child);
            }
        }

        protected virtual void BindRefs() { }

        public void Enable()
        {
            GraphicsContainer.SetActive(true);

            isEnabled = true;
        }

        public void Disable()
        {
            bool wasEnabled = isEnabled;
            isEnabled = false;

            GraphicsContainer.SetActive(false);

            if (wasEnabled && GraphicsContainer != gameObject)
                Disabled?.Invoke();
        }

        public void FadeIn(TweenData fadeData = null)
        {
            fadeData ??= FadeTweenData;

            FadeInImplementation(fadeData);
            FadeRecursive(true, transform);
        }

        public void FadeOut(TweenData fadeData = null)
        {
            fadeData ??= FadeTweenData;

            FadeOutImplementation(fadeData);
            FadeRecursive(true, transform);
        }

        protected virtual void FadeInImplementation(TweenData fadeData)
        {
            if (isFadingIn)
                return;

            if (isFadingOut)
            {
                DisposeTweens();
                isFadingOut = false;
            }

            isFadingIn = true;

            Enable();

            Tween? activeTween = null;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = spriteRenderer.color.With(a: 0.0f);
                activeTween = spriteRenderer.DoFadeTween(1.0f, fadeData);
            }

            foreach (var child in textChildren)
            {
                var childColor = child.color;

                child.color = childColor.With(a: 0.0f);
                var current = StoreTween(child.DoFadeTween(1.0f, fadeData));

                var first = GetFirstValidTween();

                if (first == current)
                    continue;

                var targetColor = childColor.With(a: 1.0f);

                first.Completed += () =>
                {
                    current.Dispose();
                    child.color = targetColor;
                };
            }

            foreach (var child in spriteChildren)
            {
                var childColor = child.color;

                child.color = childColor.With(a: 0.0f);
                var current = StoreTween(child.DoFadeTween(1.0f, fadeData));

                var first = GetFirstValidTween();

                if (first == current)
                    continue;

                var targetColor = childColor.With(a: 1.0f);

                first.Completed += () =>
                {
                    current.Dispose();
                    child.color = targetColor;
                };
            }

            void onCompleted()
            {
                isFadingIn = false;

                Disable();

                FadeOutCompleted?.Invoke();
            }

            if (activeTween == null)
                onCompleted();
            else
                activeTween.Value.Completed += onCompleted;
        }

        protected virtual void FadeOutImplementation(TweenData fadeData) { }

        private void FadeRecursive(bool fadeIn, Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                if (child.TryGetComponent(out UIElement element))
                {
                    if (fadeIn)
                        element.FadeIn();
                    else
                        element.FadeOut();
                }
                else
                    FadeRecursive(fadeIn, child);
            }
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

                if (evt.TrickleDown)
                {
                    evt.IsTricklingDown = true;

                    foreach (var child in childElements)
                    {
                        if (child == this)
                            continue;

                        child.InvokeEvent(evt);
                    }
                }

                if (evt.BubbleUp)
                {
                    evt.IsBubblingUp = true;

                    if (transform.parent == null)
                        return;

                    var parentElement = transform.parent.GetComponentInParent<UIElement>();

                    if (parentElement != null)
                        parentElement.InvokeEvent(evt);
                }
            }
        }
        #endregion

        #region Tweens
        protected Tween GetFirstValidTween() => tweenStorage.GetFirstValid();

        protected Tween StoreTween(Tween tween) => tweenStorage.Store(tween);

        protected void DisposeTweens() => tweenStorage.Dispose();
        #endregion

        #region Children
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
        #endregion

        public void Focus() => UIElementEventSystem.Focus(this);

        public void Blur() => UIElementEventSystem.Focus(null);

        #region Binding Events
        protected void Bind<T>(IReadOnlyRef<T> refVar, Action<RefChangeEvent<T>> action)
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

        protected void Unbind<T>(IReadOnlyRef<T> refVar, Action<RefChangeEvent<T>> action)
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
