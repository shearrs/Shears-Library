using System;
using System.Collections.Generic;
using Shears.Logging;
using Shears.Tweens;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    public class UIElement : SHMonoBehaviourLogger
    {
        [Flags]
        public enum RenderTargetType
        {
            Fade = 1 << 0,
            SetAlpha = 1 << 1,
            SetColor = 1 << 2,
        }

        #region Variables
        private static readonly TweenData FADE_TWEEN_DATA = new(0.1f, unscaledTime: true);

        [Header("UI Element")]
        [SerializeField, RuntimeReadOnly]
        private GameObject graphicsContainer;

        [SerializeField, RuntimeReadOnly]
        private bool useDefaultRenderTargets = true;

        [SerializeField, RuntimeReadOnly]
        private List<RenderTarget> renderTargets = new();

        private readonly Dictionary<Type, object> registrations = new();
        private readonly Dictionary<IRef, object> refBindings = new();
        private readonly Dictionary<IRef, object> rawRefBindings = new();
        private readonly List<UIElement> childElements = new();
        private readonly List<RenderTarget> newRenderTargets = new();
        private readonly TweenStorage tweenStorage = new();
        private bool isEnabled = true;
        private bool isFadingIn = false;
        private bool isFadingOut = false;
        private float dragBeginTime = 0.1f;

        protected IReadOnlyList<Tween> Tweens => tweenStorage.Tweens;
        public bool IsFadingIn => isFadingIn;
        public bool IsFadingOut => IsFadingOut;
        public bool IsFading => isFadingIn || isFadingOut;
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
        public bool IsFocused { get; internal set; }
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
        #endregion

        [Serializable]
        public class RenderTarget
        {
            private const RenderTargetType DefaultType =
                RenderTargetType.Fade | RenderTargetType.SetAlpha | RenderTargetType.SetColor;

            [SerializeField, ShowIf(nameof(graphic), compareValue1: null, nameof(uiElement), null)]
            public Renderer renderer;

            [SerializeField, ShowIf(nameof(renderer), compareValue1: null, nameof(uiElement), null)]
            public Graphic graphic;

            [SerializeField, ShowIf(nameof(renderer), compareValue1: null, nameof(graphic), null)]
            public UIElement uiElement;

            [SerializeField]
            public RenderTargetType type;

            public RenderTarget(Renderer renderer)
            {
                this.renderer = renderer;
                graphic = null;
                uiElement = null;
                type = DefaultType;
            }

            public RenderTarget(Graphic graphic)
            {
                renderer = null;
                this.graphic = graphic;
                uiElement = null;
                type = DefaultType;
            }

            public RenderTarget(UIElement uiElement)
            {
                renderer = null;
                graphic = null;
                this.uiElement = uiElement;
                type = DefaultType;
            }

            public void SetAlpha(float alpha)
            {
                if (renderer != null)
                    renderer.material.color = renderer.material.color.With(a: alpha);
                else if (graphic != null)
                    graphic.color = graphic.color.With(a: alpha);
                else if (uiElement != null)
                    uiElement.SetAlpha(alpha);
                else
                    SHLogger.Log(
                        $"{nameof(RenderTarget).PascalSpace()} had no target assigned!",
                        SHLogLevels.Error
                    );
            }

            public Tween? FadeIn(ITweenData fadeData)
            {
                if (renderer != null)
                {
                    renderer.material.color = renderer.material.color.With(a: 0.0f);

                    return renderer.DoFadeTween(1.0f, fadeData);
                }
                else if (graphic != null)
                {
                    graphic.color = graphic.color.With(a: 0.0f);

                    return graphic.DoFadeTween(1.0f, fadeData);
                }
                else if (uiElement != null)
                    return uiElement.FadeIn(fadeData);
                else
                {
                    SHLogger.Log(
                        $"{nameof(RenderTarget)} has no target to fade!",
                        SHLogLevels.Error
                    );
                    return Tween.Empty;
                }
            }

            public Tween? FadeOut(ITweenData fadeData)
            {
                if (renderer != null)
                    return renderer.DoFadeTween(0.0f, fadeData);
                else if (graphic != null)
                    return graphic.DoFadeTween(0.0f, fadeData);
                else if (uiElement != null)
                    return uiElement.FadeOut(fadeData);
                else
                {
                    SHLogger.Log(
                        $"{nameof(RenderTarget)} has no target to fade!",
                        SHLogLevels.Error
                    );
                    return Tween.Empty;
                }
            }

            public UnityEngine.Object GetTarget()
            {
                if (renderer != null)
                    return renderer;
                else if (graphic != null)
                    return graphic;
                else if (uiElement != null)
                    return uiElement;
                else
                    return null;
            }

            public Color GetColor()
            {
                if (renderer != null)
                    return renderer.material.color;
                else if (graphic != null)
                    return graphic.color;
                else if (uiElement != null)
                {
                    SHLogger.Log(
                        $"{nameof(RenderTarget)} does not support getting {nameof(UIElement)} colors!",
                        SHLogLevels.Error
                    );
                    return default;
                }
                else
                    return default;
            }

            public void SetColor(Color color)
            {
                if (renderer != null)
                    renderer.material.color = color;
                else if (graphic != null)
                    graphic.color = color;
                else if (uiElement != null)
                {
                    SHLogger.Log(
                        $"{nameof(RenderTarget)} does not support setting {nameof(UIElement)} colors!",
                        SHLogLevels.Error
                    );
                }
            }

            public Tween DoColorTween(Color color, ITweenData data)
            {
                if (renderer != null)
                    return renderer.DoColorTween(color, data);
                else if (graphic != null)
                    return graphic.DoColorTween(color, data);
                else if (uiElement != null)
                {
                    SHLogger.Log(
                        $"{nameof(RenderTarget)} does not support tweening {nameof(UIElement)}s!",
                        SHLogLevels.Error
                    );
                    return Tween.Empty;
                }
                else
                    return Tween.Empty;
            }
        }

        #region Unity Methods
        protected virtual void Awake()
        {
            UpdateChildLists();

            RegisterEvents();

            BindRefs();
        }

        protected virtual void OnDisable()
        {
            DisposeTweens();
            isFadingIn = false;
            isFadingOut = false;

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

            if (useDefaultRenderTargets)
                UpdateChildLists();
        }

        private void OnTransformChildrenChanged()
        {
            UpdateChildLists();
        }
        #endregion

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

        protected virtual void BindRefs() { }

        #region Fading
        [ContextMenu("Fade In")]
        public Tween? FadeIn() => FadeIn(FADE_TWEEN_DATA);

        public Tween? FadeIn(ITweenData fadeData)
        {
            return FadeInImplementation(fadeData);
        }

        [ContextMenu("Fade Out")]
        public Tween? FadeOut() => FadeOut(FADE_TWEEN_DATA);

        public Tween? FadeOut(ITweenData fadeData)
        {
            return FadeOutImplementation(fadeData);
        }

        public void SetAlpha(float alpha)
        {
            foreach (var target in renderTargets)
                target.SetAlpha(alpha);
        }

        protected virtual Tween? FadeInImplementation(ITweenData fadeData)
        {
            if (isFadingIn)
                return null;

            FadeInBegan?.Invoke();

            if (isFadingOut)
                isFadingOut = false;

            DisposeTweens();
            isFadingIn = true;

            Enable();

            foreach (var target in renderTargets)
            {
                var tween = target.FadeIn(fadeData);

                if (!tween.HasValue)
                    continue;

                StoreTween(tween.Value);
                var first = GetFirstValidTween();

                if (first == tween)
                    continue;

                first.Completed += () =>
                {
                    tween.Value.Dispose();
                    target.SetAlpha(1.0f);
                };
            }

            void onCompleted()
            {
                isFadingIn = false;
                FadeInCompleted?.Invoke();
            }

            var firstTween = GetFirstValidTween();

            if (firstTween == Tween.Empty)
                onCompleted();
            else
                firstTween.Completed += onCompleted;

            return firstTween;
        }

        protected virtual Tween? FadeOutImplementation(ITweenData fadeData)
        {
            if (!IsEnabled)
                return null;

            if (isFadingOut)
                return GetFirstValidTween();

            FadeOutBegan?.Invoke();

            if (isFadingIn)
                isFadingIn = false;

            DisposeTweens();
            isFadingOut = true;

            foreach (var target in renderTargets)
            {
                var tween = target.FadeOut(fadeData);

                if (!tween.HasValue)
                    continue;

                StoreTween(tween.Value);

                var first = GetFirstValidTween();

                if (first == tween)
                    continue;

                first.Completed += () =>
                {
                    tween.Value.Dispose();
                    target.SetAlpha(0.0f);
                };
            }

            void onCompleted()
            {
                isFadingOut = false;
                Disable();

                FadeOutCompleted?.Invoke();
            }

            var firstTween = GetFirstValidTween();

            if (firstTween == null)
                onCompleted();
            else
                firstTween.Completed += onCompleted;

            return firstTween;
        }
        #endregion

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

        private void UpdateChildLists()
        {
            newRenderTargets.Clear();

            if (useDefaultRenderTargets)
            {
                if (TryGetComponent(out Graphic graphic))
                    newRenderTargets.Add(new(graphic));
                if (TryGetComponent(out Renderer renderer))
                {
                    if (graphic == null || graphic is not TextMeshPro)
                        newRenderTargets.Add(new(renderer));
                }
            }

            UpdateChildListsRecursive(transform);

            foreach (var target in newRenderTargets)
            {
                if (renderTargets.Exists(r => r.GetTarget() == target.GetTarget()))
                    continue;
                else
                    renderTargets.Add(target);
            }

            for (int i = 0; i < renderTargets.Count; i++)
            {
                var target = renderTargets[i];

                if (!newRenderTargets.Exists(r => r.GetTarget() == target.GetTarget()))
                {
                    renderTargets.RemoveAt(i);
                    i--;
                }
            }
        }

        private void UpdateChildListsRecursive(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                if (child.TryGetComponent(out UIElement element))
                {
                    childElements.Add(element);

                    if (useDefaultRenderTargets)
                        newRenderTargets.Add(new(element));

                    continue;
                }

                if (useDefaultRenderTargets)
                {
                    if (child.TryGetComponent(out Graphic graphic))
                        newRenderTargets.Add(new(graphic));
                    if (child.TryGetComponent(out Renderer renderer))
                    {
                        if (graphic == null || graphic is not TextMeshPro)
                            newRenderTargets.Add(new(renderer));
                    }
                }

                UpdateChildListsRecursive(child);
            }
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
