using System;
using System.Collections.Generic;
using UnityEngine;
using static Shears.Tweens.EasingFunction;

namespace Shears.Tweens
{
    [Serializable]
    public class TweenData : ITweenData
    {
        [Header("Tween Data Settings")]
        [SerializeField] private bool usesDataObject;
        [SerializeField, ShowIf("usesDataObject")] private TweenDataObject tweenDataObject;

        [Header("Duration Settings")]
        [SerializeField, ShowIf("!usesDataObject"), Min(0f)] private float duration = 1.0f;
        [SerializeField, ShowIf("!usesDataObject")] private bool forceFinalValue = true;

        [Header("Loop Settings")]
        [SerializeField, ShowIf("!usesDataObject"), Min(-1)] private int loops = 0;
        [SerializeField, ShowIf("!usesDataObject")] private LoopMode loopMode = LoopMode.None;

        [Header("Ease Settings")]
        [SerializeField, ShowIf("!usesDataObject")] private bool usesCurve;
        [SerializeField, ShowIf("!usesDataObject", "!usesCurve")] private Ease easingFunction = Ease.Linear;
        [SerializeField, ShowIf("!usesDataObject", "usesCurve")] private AnimationCurve curve;

        [Header("Events")]
        [SerializeField] private List<TweenUnityEvent> unityEvents = new();

        private readonly List<TweenEventBase> events = new();
        private readonly List<TweenEventBase> totalEvents = new();

        public float Duration => UsesDataObject() ? tweenDataObject.Duration : duration;
        public bool ForceFinalValue => UsesDataObject() ? tweenDataObject.ForceFinalValue : forceFinalValue;
        public int Loops => UsesDataObject() ? tweenDataObject.Loops : loops;
        public LoopMode LoopMode => UsesDataObject() ? tweenDataObject.LoopMode : loopMode;
        public Ease EasingFunction => UsesDataObject() ? tweenDataObject.EasingFunction : easingFunction;
        public bool UsesCurve => usesCurve;
        public AnimationCurve Curve => curve;
        public IReadOnlyList<TweenEventBase> Events
        {
            get
            {
                totalEvents.Clear();
                totalEvents.AddRange(events);
                totalEvents.AddRange(unityEvents);

                return totalEvents;
            }
        }

        public TweenData()
        {
            duration = 1.0f;
            forceFinalValue = true;
            loops = 0;
            loopMode = LoopMode.None;
            easingFunction = Ease.Linear;
        }

        public TweenData(float duration, bool forceFinalValue = true, int loops = 0, LoopMode loopMode = LoopMode.None, Ease easingFunction = Ease.Linear)
        {
            this.duration = duration;
            this.forceFinalValue = forceFinalValue;
            this.loops = loops;
            this.loopMode = loopMode;
            this.easingFunction = easingFunction;
        }

        private bool UsesDataObject() => usesDataObject && tweenDataObject != null;

        public void SetDataObject(TweenDataObject dataObject)
        {
            tweenDataObject = dataObject;
            usesDataObject = dataObject != null;
        }

        public void AddEvent(TweenEventBase evt)
        {
            events.Add(evt);
        }

        public void RemoveEvent(TweenEventBase evt)
        {
            events.Remove(evt);
        }
    }
}
