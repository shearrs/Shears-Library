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
        [ReadOnly, SerializeField] private float progress = 0;
        [ReadOnly, SerializeField] private bool forceFinalValue;
        [ReadOnly, SerializeField] private int loops;
        [ReadOnly, SerializeField] private LoopMode loopMode;
        [ReadOnly, SerializeField] private bool reversed;
        [ReadOnly, SerializeField] private EasingFunction.Function easingFunction;
        private readonly List<Action> onCompletes = new();
        private readonly List<TweenStopEvent> stopEvents = new();
        private readonly List<TweenStopEvent> disposeEvents = new();

        private readonly List<Coroutine> coroutines = new();

        [field: ReadOnly, SerializeField] internal bool IsActive { get; set; }
        internal Action<Tween> Release { get; set; }
        internal Action<float> Update { get; set; }
        public bool IsValid => IsActive;
        public float Duration { get; private set; }
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
                    }

                    if (loopMode == LoopMode.PingPong)
                        reversed = !reversed;

                    if (loops > -1)
                        loops--;
                }

                if (loops > 1 || loops == -1)
                    yield return null;
            }

            DoOnCompletes();
            onCompletes.Clear();

            Stop();
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

                float s = GetStartValue();
                float e = GetEndValue();

                t = easingFunction(s, e, t);

                Update?.Invoke(t);

                progress += Time.deltaTime;

                yield return null;
            }
        }
        #endregion

        #region Events
        public void AddOnComplete(Action onComplete) => onCompletes.Add(onComplete);
        public void RemoveOnComplete(Action onComplete) => onCompletes.Remove(onComplete);
        public void ClearOnCompletes() => onCompletes.Clear();

        public void AddStopEvent(TweenStopEvent evt) => stopEvents.Add(evt);
        public void RemoveStopEvent(TweenStopEvent evt) => stopEvents.Remove(evt);
        public void ClearStopEvents() => stopEvents.Clear();

        public void AddDisposeEvent(TweenStopEvent evt) => disposeEvents.Add(evt);
        public void RemoveDisposeEvent(TweenStopEvent evt) => disposeEvents.Remove(evt);
        public void ClearDisposeEvents() => disposeEvents.Clear();

        private void DoOnCompletes()
        {
            foreach (Action action in onCompletes)
                action?.Invoke();
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

        internal void SetData(TweenData data)
        {
            Duration = data.Duration;
            forceFinalValue = data.ForceFinalValue;
            loops = data.Loops;
            loopMode = data.LoopMode;
            reversed = false;
            easingFunction = EasingFunction.GetEasingFunction(data.EasingFunction);

            if (loops > -1)
                loops++;
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
