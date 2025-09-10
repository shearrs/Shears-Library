using System.Collections.Generic;
using UnityEngine;
using static Shears.Tweens.EasingFunction;

namespace Shears.Tweens
{
    [System.Serializable]
    public struct StructTweenData : ITweenData
    {
#if UNITY_EDITOR
        [HideInInspector, SerializeField] private bool isExpanded;
#endif

        [Header("Tween Data Settings")]
        [SerializeField] private bool usesDataObject;
        [SerializeField, ShowIf("usesDataObject")] private TweenDataObject tweenDataObject;

        [Header("Duration Settings")]
        [SerializeField, ShowIf("!usesDataObject")] private float duration;
        [SerializeField, ShowIf("!usesDataObject")] private bool forceFinalValue;

        [Header("Loop Settings")]
        [SerializeField, ShowIf("!usesDataObject")] private int loops;
        [SerializeField, ShowIf("!usesDataObject")] private LoopMode loopMode;

        [Header("Ease Settings")]
        [SerializeField, ShowIf("!usesDataObject")] private bool usesCurve;
        [SerializeField, ShowIf("!usesDataObject", "!usesCurve")] private Ease easingFunction;
        [SerializeField, ShowIf("!usesDataObject", "usesCurve")] private AnimationCurve curve;

        [Header("Events")]
        [SerializeField] private List<TweenUnityEvent> unityEvents;

        private List<TweenEventBase> events;
        private List<TweenEventBase> totalEvents;

        public readonly float Duration => UsesDataObject() ? tweenDataObject.Duration : duration;
        public readonly bool ForceFinalValue => UsesDataObject() ? tweenDataObject.ForceFinalValue : forceFinalValue;
        public readonly int Loops => UsesDataObject() ? tweenDataObject.Loops : loops;
        public readonly LoopMode LoopMode => UsesDataObject() ? tweenDataObject.LoopMode : loopMode;
        public readonly Ease EasingFunction => UsesDataObject() ? tweenDataObject.EasingFunction : easingFunction;
        public readonly bool UsesCurve => usesCurve;
        public readonly AnimationCurve Curve => curve;
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

        public StructTweenData(float duration = 1.0f, bool forceFinalValue = true, int loops = 0, LoopMode loopMode = LoopMode.None, Ease easingFunction = Ease.Linear)
        {
            usesDataObject = false;
            tweenDataObject = null;

            this.duration = duration;
            this.forceFinalValue = forceFinalValue;
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

        public StructTweenData(float duration = 1.0f, bool forceFinalValue = true, int loops = 0, LoopMode loopMode = LoopMode.None, AnimationCurve curve = null)
        {
            usesDataObject = false;
            tweenDataObject = null;

            this.duration = duration;
            this.forceFinalValue = forceFinalValue;
            this.loops = loops;
            this.loopMode = loopMode;
            easingFunction = Ease.Linear;
            usesCurve = true;
            this.curve = curve;

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
