using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Tweens
{
    public enum LoopMode
    {
        None,
        Repeat,
        PingPong
    }

    public delegate bool TweenStopEvent();

    [Serializable]
    public class Tween
    {
        [Header("Duration")]
        [ReadOnly, SerializeField] private float progress = 0;
        [ReadOnly, SerializeField] private bool forceFinalValue;

        [Header("Loops")]
        [ReadOnly, SerializeField] private int loops;
        [ReadOnly, SerializeField] private LoopMode loopMode;
        [ReadOnly, SerializeField] private bool reversed;

        [Header("Easing")]
        [ReadOnly, SerializeField] private bool usesCurve;
        [ReadOnly, SerializeField] private EasingFunction.Function easingFunction;
        [ReadOnly, SerializeField] private AnimationCurve curve;

        [Header("Events")]
        [ReadOnly, SerializeField] private List<TweenEventBase> events = new();

        private bool isInvokingEvents = false;
        private bool disposeAfterEvents = false;
        private readonly List<Action> onCompletes = new();
        private readonly List<TweenEventBase> activeEvents = new();
        private readonly List<TweenEventBase> eventsToClear = new();
        private readonly List<TweenStopEvent> stopEvents = new();
        private readonly List<TweenStopEvent> disposeEvents = new();
        private readonly List<Coroutine> coroutines = new();

        [field: ReadOnly, SerializeField] internal bool IsActive { get; set; }
        internal Action<Tween> Release { get; set; }
        internal Action<float> Update { get; set; }
        public bool IsValid => IsActive;
        public float Duration { get; private set; }
        public float Progress => Mathf.Abs(progress / Duration);
        public bool IsPlaying { get; private set; }
        public bool Paused { get; private set; }
        public int Loops
        {
            get
            {
                if (loops == -1)
                    return -1;
                else
                    return loops + 1;
            }
        }

        internal Tween()
        {
            Application.quitting += Dispose;
        }

        ~Tween()
        {
            Application.quitting -= Dispose;
        }

        public void Play()
        {
            if (IsPlaying)
                return;

            StopAllCoroutines();

            progress = 0;
            coroutines.Add(StartCoroutine(IEPlay()));
        }

        public void PlayAfter(float seconds)
        {
            if (IsPlaying)
                return;

            StopAllCoroutines();
            StartCoroutine(IEPlayAfter(seconds));
        }

        public void Stop()
        {
            if (!IsPlaying)
                return;

            IsPlaying = false;
            StopAllCoroutines();
        }

        public void Pause()
        {
            IsPlaying = false;
            Paused = true;
        }

        public void Dispose()
        {
            if (!IsActive)
                return;
            else if (isInvokingEvents)
            {
                disposeAfterEvents = true;
                return;
            }

            disposeAfterEvents = false;

            Stop();
            Release?.Invoke(this);
        }

        #region Playing Enumerators
        private IEnumerator IEPlay()
        {
            IsPlaying = true;

            while (loops > 0 || loops == -1)
            {
                progress = 0;

                Coroutine updateCoroutine = StartCoroutine(IEUpdate());
                coroutines.Add(updateCoroutine);

                yield return updateCoroutine;

                coroutines.Remove(updateCoroutine);

                if (IsPlaying && !EvaluateStopAndDisposeEvents())
                {
                    if (forceFinalValue)
                    {
                        progress = GetEndValue();

                        Update?.Invoke(progress);
                        UpdateEvents(1.0f);
                    }

                    if (loopMode == LoopMode.PingPong)
                        reversed = !reversed;

                    if (loops > -1)
                        loops--;
                }

                if (loops > 1 || loops == -1)
                    yield return null;
            }

            InvokeOnCompletes();
            onCompletes.Clear();

            Stop();
        }

        private IEnumerator IEPlayAfter(float seconds)
        {
            if (seconds > 0)
                yield return CoroutineUtil.WaitForSeconds(seconds);

            Play();
        }

        private IEnumerator IEUpdate()
        {
            while (progress <= Duration)
            {
                while (Paused)
                {
                    if (EvaluateStopAndDisposeEvents())
                        yield break;

                    yield return null;
                }

                if (EvaluateStopAndDisposeEvents())
                    yield break;

                float t;

                if (Duration == 0)
                    t = 1;
                else
                    t = progress / Duration;

                if (usesCurve)
                    t = curve.Evaluate(t);
                else
                {
                    float s = GetStartValue();
                    float e = GetEndValue();

                    t = easingFunction(s, e, t);
                }

                Update?.Invoke(t);
                UpdateEvents(t);

                progress += Time.deltaTime;

                yield return null;
            }
        }
        #endregion

        #region Events
        public void AddOnComplete(Action onComplete) => onCompletes.Add(onComplete);
        public void RemoveOnComplete(Action onComplete) => onCompletes.Remove(onComplete);
        public void ClearOnCompletes() => onCompletes.Clear();

        public void AddEvent(TweenEventBase tweenEvent) => events.Add(tweenEvent);
        public void AddEvent(float progress, Action callback)
        {
            var evt = new TweenEvent(progress);
            evt.ProgressReached += callback;

            events.Add(evt);
            activeEvents.Add(evt);
        }
        public void RemoveEvent(TweenEventBase tweenEvent) => events.Remove(tweenEvent);
        public void ClearEvents() => events.Clear();

        public void AddStopEvent(TweenStopEvent evt) => stopEvents.Add(evt);
        public void RemoveStopEvent(TweenStopEvent evt) => stopEvents.Remove(evt);
        public void ClearStopEvents() => stopEvents.Clear();

        public void AddDisposeEvent(TweenStopEvent evt) => disposeEvents.Add(evt);
        public void RemoveDisposeEvent(TweenStopEvent evt) => disposeEvents.Remove(evt);
        public void ClearDisposeEvents() => disposeEvents.Clear();

        private void InvokeOnCompletes()
        {
            isInvokingEvents = true;

            foreach (Action action in onCompletes)
                action?.Invoke();

            isInvokingEvents = false;

            if (disposeAfterEvents)
                Dispose();
        }

        private bool EvaluateStopAndDisposeEvents()
        {
            bool stop = false;

            foreach (var evt in stopEvents)
            {
                if (evt())
                {
                    Stop();
                    stop = true;

                    break;
                }
            }

            foreach (var evt in disposeEvents)
            {
                if (evt())
                {
                    Dispose();
                    stop = true;

                    break;
                }
            }

            return stop;
        }
        #endregion

        internal void SetData(ITweenData data)
        {
            Duration = data.Duration;
            forceFinalValue = data.ForceFinalValue;
            loops = data.Loops;
            loopMode = data.LoopMode;
            reversed = false;
            easingFunction = EasingFunction.GetEasingFunction(data.EasingFunction);
            usesCurve = data.UsesCurve;
            curve = data.Curve;

            InitializeEvents(data);

            if (loops > -1)
                loops++;
        }

        private void InitializeEvents(ITweenData data)
        {
            events.Clear();
            events.AddRange(data.Events);
            activeEvents.AddRange(data.Events);
        }

        private void UpdateEvents(float t)
        {
            isInvokingEvents = true;
            eventsToClear.Clear();

            foreach (var evt in activeEvents)
            {
                if (evt.CanInvoke(t))
                {
                    evt.Invoke();
                    eventsToClear.Add(evt);
                }
            }

            foreach (var evt in eventsToClear)
                activeEvents.Remove(evt);

            isInvokingEvents = false;

            if (disposeAfterEvents)
                Dispose();
        }

        #region Utility
        private void StopAllCoroutines()
        {
            int count = coroutines.Count;

            for (int i = 0; i < count; i++)
            {
                if (coroutines[0] != null)
                    CoroutineRunner.Stop(coroutines[0]);

                coroutines.RemoveAt(0);
            }
        }
        private Coroutine StartCoroutine(IEnumerator routine) => CoroutineRunner.Start(routine);
        private float GetStartValue() => reversed ? 1 : 0;
        private float GetEndValue() => reversed ? 0 : 1;
        #endregion
    }
}
