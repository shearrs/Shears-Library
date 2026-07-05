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
        private delegate int GetSortOrderCallback();
        private delegate void GetChildrenCallback(List<UIElement> children);

        #region Variables
        [NonSerialized]
        private List<UIElement> flattenedHierarchy;

        [NonSerialized]
        private List<GameObject> rootObjects;

        private readonly Dictionary<Type, object> registrations = new();
        private readonly Dictionary<IRef, object> refBindings = new();
        private readonly Dictionary<IRef, object> rawRefBindings = new();
        private readonly List<UIElement> children = new();
        private readonly List<UIElement> tempElements = new();
        private readonly TweenStorage tweenStorage = new();
        private readonly Ref<bool> isFocused = new();
        private Dictionary<UIElement, int> hierarchyIndex;
        private bool isHierarchyInitialized;
        private float dragBeginTime = 0.1f;

        private int Depth { get; set; }
        private UIElement Parent { get; set; }
        private UIElementCanvas UICanvas { get; set; }
        private GetChildrenCallback GetChildren { get; set; }
        private GetSortOrderCallback GetSortOrder { get; set; }
        protected IReadOnlyList<Tween> Tweens => tweenStorage.Tweens;
        public IReadOnlyList<UIElement> Children
        {
            get
            {
                if (!isHierarchyInitialized)
                    ForceInitializeHierarchy();

                GetChildren(children);
                return children;
            }
        }
        public bool IsEnabled => isActiveAndEnabled;
        public bool IsFocused
        {
            get => isFocused;
            internal set => isFocused.Value = value;
        }
        public IReadOnlyRef<bool> IsFocusedRef => isFocused;
        public bool HasCanvasParent => UICanvas != null;
        public float DragBeginTime
        {
            get => dragBeginTime;
            set => dragBeginTime = value;
        }
        public int RootSortOrder { get; private set; }
        public int SortOrder
        {
            get
            {
                if (!isHierarchyInitialized)
                    ForceInitializeHierarchy();

                return GetSortOrder();
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
        private event Action Destroyed;
        private event Action ParentChanged;
        private event Action ChildrenChanged;
        #endregion

        #region Unity Methods
        protected virtual void Awake()
        {
            if (
                !isHierarchyInitialized
                && (transform.parent == null || !transform.parent.TryGetComponent(out UIElement _))
            )
                UpdateHierarchy();

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
            Destroyed?.Invoke();

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
            if (transform.parent == null || !transform.TryGetComponent(out UIElement _))
                UpdateHierarchy();
            else if (flattenedHierarchy != null && flattenedHierarchy.Count > 0)
                flattenedHierarchy.Clear();

            ParentChanged?.Invoke();
        }

        private void OnTransformChildrenChanged()
        {
            ChildrenChanged?.Invoke();
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

            if (evt.IsTricklingDown)
                return;

            if (evt.TrickleDown && !evt.IsBubblingUp)
            {
                evt.IsTricklingDown = true;
                GetChildren(tempElements);

                foreach (var child in tempElements)
                {
                    if (child == this)
                        continue;

                    child.InvokeEvent(evt);
                }

                evt.IsTricklingDown = false;
            }

            if (evt.BubbleUp)
            {
                evt.IsBubblingUp = true;

                if (Parent == null)
                    return;

                Parent.InvokeEvent(evt);

                evt.IsBubblingUp = false;
            }
        }
        #endregion

        #region Tweens
        protected Tween GetFirstValidTween() => tweenStorage.GetFirstValid();

        protected Tween StoreTween(in Tween tween) => tweenStorage.Store(tween);

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

        private void ForceInitializeHierarchy()
        {
            if (ApplicationUtil.IsQuitting || isHierarchyInitialized)
                return;

            var targetTransform = transform;
            var targetElement = this;

            while (
                targetTransform.parent != null
                && targetTransform.parent.TryGetComponent(out UIElement element)
            )
            {
                targetTransform = targetTransform.parent;
                targetElement = element;
            }

            targetElement.UpdateHierarchy();
        }

        private void UpdateHierarchy()
        {
            if (ApplicationUtil.IsQuitting)
                return;

            if (flattenedHierarchy != null)
            {
                foreach (var element in flattenedHierarchy)
                {
                    element.ParentChanged -= UpdateHierarchy;
                    element.ChildrenChanged -= UpdateHierarchy;
                    element.Destroyed -= UpdateHierarchy;
                }

                flattenedHierarchy.Clear();
                hierarchyIndex.Clear();
            }
            else
            {
                flattenedHierarchy = new();
                hierarchyIndex = new();
            }

            if (transform.parent == null)
            {
                rootObjects ??= new();
                gameObject.scene.GetRootGameObjects(rootObjects);

                RootSortOrder = rootObjects.IndexOf(gameObject);
            }
            else
            {
                int depth = 0;
                var parent = transform.parent;

                while (parent != null)
                {
                    depth++;
                    parent = parent.parent;
                }

                depth += transform.GetSiblingIndex();

                RootSortOrder = depth;
            }

            AddHierarchyElement(this);

            if (TryGetComponent(out UIElementCanvas canvas))
                UICanvas = canvas;

            UpdateHierarchy(this);
        }

        private void UpdateHierarchy(UIElement element, UIElementCanvas canvas = null)
        {
            if (TryGetComponent(out UIElementCanvas possibleCanvas))
                canvas = possibleCanvas;

            for (int i = 0; i < element.transform.childCount; i++)
            {
                var child = element.transform.GetChild(i);

                if (!child.TryGetComponent(out UIElement childElement))
                    continue;

                AddHierarchyElement(childElement, element, canvas);
                UpdateHierarchy(childElement, canvas);
            }
        }

        private void AddHierarchyElement(
            UIElement element,
            UIElement parent = null,
            UIElementCanvas canvas = null
        )
        {
            flattenedHierarchy.Add(element);
            hierarchyIndex.Add(element, flattenedHierarchy.Count - 1);
            element.GetSortOrder = () => GetHierarchySortOrder(element);
            element.GetChildren = (list) => GetHierarchyChildren(element, list);
            element.Parent = parent;
            element.UICanvas = canvas;
            element.RootSortOrder = RootSortOrder;
            element.isHierarchyInitialized = true;

            if (element.Parent != null)
                element.Depth = element.Parent.Depth + 1;
            else
                element.Depth = 0;

            if (element != this)
            {
                element.ParentChanged += UpdateHierarchy;
                element.ChildrenChanged += UpdateHierarchy;
                element.Destroyed += UpdateHierarchy;
            }
        }

        private int GetHierarchySortOrder(UIElement element)
        {
            if (hierarchyIndex.TryGetValue(element, out int order))
                return order;
            else
            {
                Log($"{nameof(UIElement)} {name} failed to fetch sort order.", SHLogLevels.Error);
                return 0;
            }
        }

        private void GetHierarchyChildren(UIElement element, List<UIElement> children)
        {
            children.Clear();

            if (!hierarchyIndex.TryGetValue(element, out var index))
            {
                Log(
                    $"{nameof(UIElement)} {name} failed to fetch hierarchy index.",
                    SHLogLevels.Error
                );
                return;
            }

            if (index == 0)
            {
                for (int i = 1; i < flattenedHierarchy.Count; i++)
                    children.Add(flattenedHierarchy[i]);
            }
            else
            {
                for (int i = index + 1; i < flattenedHierarchy.Count; i++)
                {
                    var child = flattenedHierarchy[i];

                    if (child.Depth <= element.Depth) // We are on a sibling or parent
                        break;

                    children.Add(child);
                }
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
