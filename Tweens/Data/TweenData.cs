using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Tweens
{
    [Serializable]
    public class TweenData : ITweenData
    {
        #region Variables
        [Header("Tween Data Settings")]
        [SerializeField] private bool usesDataObject;
        [SerializeField, ShowIf("usesDataObject")] private TweenDataObject tweenDataObject;

        [Header("Duration Settings")]
        [SerializeField, ShowIf("!usesDataObject"), Min(0f)] private float duration = 1.0f;
        [SerializeField, ShowIf("!usesDataObject")] private bool forceFinalValue = true;
        [SerializeField, ShowIf("!usesDataObject")] private Tween.UpdateMode updateMode;
        [SerializeField, ShowIf("!usesDataObject")] private bool unscaledTime = false;

        [Header("Loop Settings")]
        [SerializeField, ShowIf("!usesDataObject"), Min(-1)] private int loops = 0;
        [SerializeField, ShowIf("!usesDataObject")] private LoopMode loopMode = LoopMode.None;

        [Header("Ease Settings")]
        [SerializeField, ShowIf("!usesDataObject")] private bool usesCurve;
        [SerializeField, ShowIf("!usesDataObject", "!usesCurve")] private TweenEase easingFunction = TweenEase.Linear;
        [SerializeField, ShowIf("!usesDataObject", "usesCurve")] private AnimationCurve curve;

        [Header("Events")]
        [SerializeField] private List<TweenUnityEvent> unityEvents = new();

        private readonly List<TweenEventBase> events = new();
        private readonly List<TweenEventBase> totalEvents = new();

        public float Duration
        {
            get => UsesDataObject()? tweenDataObject.Duration : duration;
            set => duration = value;
        }

        public bool ForceFinalValue
        {
            get => UsesDataObject() ? tweenDataObject.ForceFinalValue : forceFinalValue;
            set => forceFinalValue = value;
        }

        public Tween.UpdateMode UpdateMode
        {
            get => UsesDataObject() ? tweenDataObject.UpdateMode : updateMode;
            set => updateMode = value;
        }

        public bool UnscaledTime
        {
            get => UsesDataObject() ? tweenDataObject.UnscaledTime : unscaledTime;
            set => unscaledTime = value;
        }

        public int Loops
        { 
            get => UsesDataObject() ? tweenDataObject.Loops : loops;
            set => loops = value;
        }

        public LoopMode LoopMode
        {
            get => UsesDataObject() ? tweenDataObject.LoopMode : loopMode;
            set => loopMode = value;
        }

        public TweenEase EasingFunction
        {
            get => UsesDataObject() ? tweenDataObject.EasingFunction : easingFunction;
            set => easingFunction = value;
        }

        public bool UsesCurve
        {
            get => usesCurve;
            set => usesCurve = value;
        }

        public AnimationCurve Curve
        {
            get => curve;
            set => curve = value;
        }

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
        #endregion

        public TweenData()
        {
            duration = 1.0f;
            forceFinalValue = true;
            loops = 0;
            loopMode = LoopMode.None;
            easingFunction = TweenEase.Linear;
        }

        public TweenData(
            float duration = 1.0f, bool forceFinalValue = true,
            Tween.UpdateMode updateMode = Tween.UpdateMode.Update,
            bool unscaledTime = false, int loops = 0,
            LoopMode loopMode = LoopMode.None,
            TweenEase easingFunction = TweenEase.Linear)
        {
            this.duration = duration;
            this.forceFinalValue = forceFinalValue;
            this.updateMode = updateMode;
            this.unscaledTime = unscaledTime;
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
