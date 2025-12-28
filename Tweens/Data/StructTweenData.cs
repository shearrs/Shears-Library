using System.Collections.Generic;
using UnityEngine;

namespace Shears.Tweens
{
    [System.Serializable]
    public struct StructTweenData : ITweenData
    {
#if UNITY_EDITOR
#pragma warning disable CS0414
        [HideInInspector, SerializeField] private bool isExpanded;
#pragma warning restore CS0414
#endif

        [Header("Tween Data Settings")]
        [SerializeField] private bool usesDataObject;
        [SerializeField] private TweenDataObject tweenDataObject;

        [Header("Duration Settings")]
        [SerializeField, Min(0f)] private float duration;
        [SerializeField] private bool forceFinalValue;
        [SerializeField] private Tween.UpdateMode updateMode;
        [SerializeField] private bool unscaledTime;

        [Header("Loop Settings")]
        [SerializeField, Min(-1)] private int loops;
        [SerializeField] private LoopMode loopMode;

        [Header("Ease Settings")]
        [SerializeField] private bool usesCurve;
        [SerializeField] private TweenEase easingFunction;
        [SerializeField] private AnimationCurve curve;

        [Header("Events")]
        [SerializeField] private List<TweenUnityEvent> unityEvents;

        private List<TweenEventBase> events;
        private List<TweenEventBase> totalEvents;

        public float Duration { readonly get => UsesDataObject() ? tweenDataObject.Duration : duration; set => duration = value; }
        public bool ForceFinalValue { readonly get => UsesDataObject() ? tweenDataObject.ForceFinalValue : forceFinalValue; set => forceFinalValue = value; }
        public Tween.UpdateMode UpdateMode { readonly get => UsesDataObject() ? tweenDataObject.UpdateMode : updateMode; set => updateMode = value; }
        public bool UnscaledTime { readonly get => UsesDataObject() ? tweenDataObject.UnscaledTime : unscaledTime; set => unscaledTime = value; }
        public int Loops { readonly get => UsesDataObject() ? tweenDataObject.Loops : loops; set => loops = value; }
        public LoopMode LoopMode { readonly get => UsesDataObject() ? tweenDataObject.LoopMode : loopMode; set => loopMode = value; }
        public TweenEase EasingFunction { readonly get => UsesDataObject() ? tweenDataObject.EasingFunction : easingFunction; set => easingFunction = value; }
        public bool UsesCurve { readonly get => usesCurve; set => usesCurve = value; }
        public AnimationCurve Curve { readonly get => curve; set => curve = value; }
        public IReadOnlyList<TweenEventBase> Events
        {
            get
            {
                events ??= new();
                totalEvents ??= new();

                totalEvents.Clear();
                totalEvents.AddRange(events);
                totalEvents.AddRange(unityEvents);

                return totalEvents;
            }
        }

        public StructTweenData(
            float duration = 1.0f, bool forceFinalValue = true, 
            Tween.UpdateMode updateMode = Tween.UpdateMode.Update, 
            bool unscaledTime = false, int loops = 0, 
            LoopMode loopMode = LoopMode.None, 
            TweenEase easingFunction = TweenEase.Linear)
        {
            usesDataObject = false;
            tweenDataObject = null;

            this.duration = duration;
            this.forceFinalValue = forceFinalValue;
            this.updateMode = updateMode;
            this.unscaledTime = false;
            this.loops = loops;
            this.loopMode = loopMode;
            this.easingFunction = easingFunction;
            usesCurve = false;
            curve = AnimationCurve.Linear(0, 0, 1, 1);

            events = new();
            unityEvents = new();
            totalEvents = new();

#if UNITY_EDITOR
            isExpanded = false;
#endif
        }

        public void SetDataObject(TweenDataObject dataObject)
        {
            tweenDataObject = dataObject;
            usesDataObject = dataObject != null;
        }

        public void AddEvent(TweenEventBase evt)
        {
            events ??= new();

            events.Add(evt);
        }

        public readonly void RemoveEvent(TweenEventBase evt)
        {
            events?.Remove(evt);
        }

        private readonly bool UsesDataObject() => usesDataObject && tweenDataObject != null;
    }
}
