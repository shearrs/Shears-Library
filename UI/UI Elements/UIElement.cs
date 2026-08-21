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
        [Header("UIElement")]
        [SerializeField, Range(0, 1)]
        private float alpha = 1.0f;

        private readonly Dictionary<Type, object> registrations = new();
        private readonly Dictionary<IRef, object> refBindings = new();
        private readonly Dictionary<IRef, object> rawRefBindings = new();
        private readonly List<UIElement> children = new();
        private readonly TweenStorage tweenStorage = new();
        private readonly Ref<bool> isFocused = new();
        private bool isDirty = false;
        private int rootSortOrder;
        private float dragBeginTime = 0.1f;

        private DataTree<UIElement> Hierarchy { get; set; }
        private UIElementCanvas UICanvas { get; set; }
        protected IReadOnlyList<Tween> Tweens => tweenStorage.Tweens;
        public int Depth => GetDepth();
        public UIElement Parent => GetParent();
        public IReadOnlyList<UIElement> Children
        {
            get
            {
                GetChildren();

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
        public int RootSortOrder => GetRootSortOrder();
        public int SortOrder => GetSortOrder();
        public Color BaseColor
        {
            get => BaseColorValue;
            set
            {
                if (BaseColorValue == value)
                    return;

                MarkDirty();
                BaseColorValue = value;
            }
        }
        public Color Modulate
        {
            get => ModulateValue;
            set
            {
                if (ModulateValue == value)
                    return;

                MarkDirty();
                ModulateValue = value;
            }
        }
        public float Alpha
        {
            get => AlphaValue;
            set
            {
                if (AlphaValue == value)
                    return;

                MarkDirty();
                AlphaValue = value;
            }
        }
        public bool AdditiveModulate
        {
            get => AdditiveModulateValue;
            set
            {
                if (AdditiveModulateValue == value)
                    return;

                MarkDirty();
                AdditiveModulateValue = value;
            }
        }
        protected virtual Color BaseColorValue { get; set; } = Color.white;
        protected virtual Color ModulateValue { get; set; } = Color.white;
        protected virtual float AlphaValue
        {
            get => alpha;
            set => alpha = value;
        }
        protected virtual bool AdditiveModulateValue { get; set; } = false;

        public event Action Disabled;
        #endregion

        #region Unity Methods
        protected virtual void Awake()
        {
            UIElementEventSystem.CreateInstanceIfNoneExists();

            if (
                Hierarchy == null
                && (transform.parent == null || !transform.parent.TryGetComponent(out UIElement _))
            )
                CreateHierarchy();

            RegisterEvents();
            BindRefs();

            if (Hierarchy != null)
                MarkDirty();
        }

        protected virtual void OnDisable()
        {
            DisposeTweens();

            if (ApplicationUtil.IsQuitting)
                return;

            CalculateAndApplyStyle();

            Disabled?.Invoke();
        }

        protected virtual void OnDestroy()
        {
            OnHierarchyDestroyed();
            Unbind();
        }

        protected virtual void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (gameObject.layer != LayerMask.NameToLayer("UI"))
                Invoke(nameof(SetLayer), 0f);
        }

        private void OnTransformParentChanged()
        {
            OnHierarchyParentChanged();
        }

        private void OnTransformChildrenChanged()
        {
            OnHierarchyChildChanged();
            MarkDirty();
        }

        private void LateUpdate()
        {
            if (isDirty)
                CalculateAndApplyStyle();
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

        private void CalculateAndApplyStyle()
        {
            var baseColor = BaseColor;
            var modulate = Modulate;
            float alpha = baseColor.a * modulate.a * Alpha;

            var parent = Parent;
            while (parent != null)
            {
                alpha *= parent.Alpha;

                parent = parent.Parent;
            }

            Color modColor;

            if (AdditiveModulate)
                modColor = baseColor + modulate;
            else
                modColor = modulate * baseColor;

            var resolvedColor = modColor.With(a: alpha);

            isDirty = false;
            Repaint(new(resolvedColor));

            foreach (var child in Children)
            {
                if (child.isDirty)
                    child.CalculateAndApplyStyle();
            }
        }

        protected virtual void Repaint(StyleData data) { }

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

        public void UnregisterEvent<EventType>(Action<EventType> callback)
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
        }
        #endregion

        #region Tweens
        protected Tween GetFirstValidTween() => tweenStorage.GetFirstValid();

        protected Tween StoreTween(in Tween tween) => tweenStorage.Store(tween);

        protected void DisposeTweens() => tweenStorage.Dispose();

        protected void AddTweenStorage(TweenStorage storage) => tweenStorage.AddSubStorage(storage);

        protected void RemoveTweenStorage(TweenStorage storage) =>
            tweenStorage.RemoveSubStorage(storage);

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

        #region Hierarchy
        internal UIElement GetHighestSortOrderChild()
        {
            if (Hierarchy == null)
                return this;
            else
            {
                var children = Children;

                if (children.Count == 0)
                    return this;
                else
                {
                    int maxOrder = 0;
                    UIElement targetChild = this;

                    foreach (var child in children)
                    {
                        if (child.SortOrder > maxOrder)
                        {
                            maxOrder = child.SortOrder;
                            targetChild = child;
                        }
                    }

                    return targetChild;
                }
            }
        }

        private void CreateHierarchy()
        {
            Hierarchy = new();

            CreateHierarchyRecursive(this, null);

            if (transform.parent == null)
            {
                CollectionUtil.GetPooled(out List<GameObject> rootObjects);
                gameObject.scene.GetRootGameObjects(rootObjects);

                rootSortOrder = rootObjects.IndexOf(gameObject);

                CollectionUtil.ReleasePooled(rootObjects);
            }
            else
                rootSortOrder = transform.GetSiblingIndex();
        }

        private void CreateHierarchyRecursive(UIElement element, UIElement parent)
        {
            if (element.TryGetComponent(out UIElementCanvas canvas))
                element.UICanvas = canvas;
            else if (parent != null)
                element.UICanvas = parent.UICanvas;

            element.Hierarchy = Hierarchy;
            Hierarchy.Add(element, parent);

            for (int i = 0; i < element.transform.childCount; i++)
            {
                var child = element.transform.GetChild(i);
                var childElement = child.GetComponent<UIElement>();

                CreateHierarchyRecursive(childElement, element);
            }
        }

        private void Add(UIElement element)
        {
            Hierarchy.GetDirectChildren(this, children);
            InsertChildRecursive(children.Count, this, element);
        }

        private void Insert(int index, UIElement element)
        {
            InsertChildRecursive(index, this, element);
        }

        private UIElement GetParent()
        {
            if (Hierarchy == null)
                return null;
            else
                return Hierarchy.GetParent(this);
        }

        private void GetChildren()
        {
            children.Clear();

            if (Hierarchy == null)
                return;
            else
                Hierarchy.GetChildren(this, children);
        }

        private int GetDepth()
        {
            if (Hierarchy == null)
                return 0;
            else
                return Hierarchy.GetDepth(this);
        }

        private int GetSortOrder()
        {
            if (Hierarchy == null)
                return 0;
            else
                return Hierarchy.GetOrder(this);
        }

        private int GetRootSortOrder()
        {
            if (Hierarchy == null || Hierarchy.Root == null)
                return 0;
            else
            {
                if (Hierarchy.Root == this)
                    return rootSortOrder;
                else
                    return Hierarchy.Root.SortOrder;
            }
        }

        private void OnHierarchyParentChanged()
        {
            Hierarchy?.Remove(this);
            Hierarchy = null;

            var parent = transform.parent;

            if (parent == null || !parent.TryGetComponent(out UIElement _))
                CreateHierarchy();
        }

        private void OnHierarchyChildChanged()
        {
            if (Hierarchy == null)
                return;

            Hierarchy.GetDirectChildren(this, children);

            foreach (var child in children)
                Hierarchy.Remove(child);

            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                var childElement = child.GetComponent<UIElement>();

                Insert(i, childElement);
            }
        }

        private void OnHierarchyDestroyed()
        {
            Hierarchy?.Remove(this);
        }

        private void InsertChildRecursive(int index, UIElement parent, UIElement element)
        {
            if (element.TryGetComponent(out UIElementCanvas canvas))
                element.UICanvas = canvas;
            else if (parent != null)
                element.UICanvas = parent.UICanvas;

            element.Hierarchy = Hierarchy;
            Hierarchy.Insert(index, parent, element);

            for (int i = 0; i < element.transform.childCount; i++)
            {
                var child = element.transform.GetChild(i);
                var childElement = child.GetComponent<UIElement>();

                InsertChildRecursive(i, element, childElement);
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

        private void MarkDirty()
        {
            isDirty = true;

            foreach (var child in Children)
                child.isDirty = true;
        }
    }
}
