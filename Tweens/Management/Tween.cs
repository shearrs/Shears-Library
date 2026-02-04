using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Tweens
{
    public readonly struct Tween
    {
        public enum UpdateMode { Update, LateUpdate, FixedUpdate }

        private readonly TweenInstance tween;
        private readonly Guid id;

        #region Wrapped Tween Properties
        public readonly bool IsValid => IsTweenValid();
        public readonly float Duration => IsTweenValid() ? tween.Duration : -1;
        public readonly float Progress => IsTweenValid() ? tween.Progress : 0;
        public readonly bool IsPlaying => IsTweenValid() && tween.IsPlaying;
        public readonly bool Paused => IsTweenValid() && tween.Paused;
        public readonly int Loops => IsTweenValid() ? tween.Loops : 0;
        public readonly event Action Completed { add => AddOnComplete(value); remove => RemoveOnComplete(value); }
        public readonly Coroutine CoroutineHandle => IsTweenValid() ? tween.GetCoroutineHandle() : null;
        #endregion

        public Tween(TweenInstance tween)
        {
            this.tween = tween;
            id = tween.ID;
        }

        #region Wrapped Tween Functions
        public readonly void Play() => DoIfValid(tween != null ? tween.Play : null);
        public readonly void PlayAfter(float seconds)
        {
            if (IsTweenValid())
                tween.PlayAfter(seconds);
            else
                ErrorMessage();
        }
        public readonly void Stop() => DoIfValid(tween != null ? tween.Stop : null);
        public readonly void Pause() => DoIfValid(tween != null ? tween.Pause : null);
        public readonly void Dispose() => DoIfValid(tween != null ? tween.Dispose : null, false);

        public void AddOnComplete(Action onComplete)
        {
            if (IsTweenValid())
                tween.AddOnComplete(onComplete);
            else
                ErrorMessage();
        }

        public void RemoveOnComplete(Action onComplete)
        {
            if (IsTweenValid())
                tween.RemoveOnComplete(onComplete);
            else
                ErrorMessage();
        }

        public void ClearOnCompletes() => DoIfValid(tween != null ? tween.ClearOnCompletes : null);

        public void AddEvent(TweenEventBase tweenEvent)
        {
            if (IsTweenValid())
                tween.AddEvent(tweenEvent);
            else
                ErrorMessage();
        }

        public void AddEvent(float progress, Action callback)
        {
            if (IsTweenValid())
                tween.AddEvent(progress, callback);
            else
                ErrorMessage();
        }

        public void RemoveEvent(TweenEventBase tweenEvent)
        {
            if (IsTweenValid())
                tween.RemoveEvent(tweenEvent);
            else
                ErrorMessage();
        }

        public void ClearEvents() => DoIfValid(tween != null ? tween.ClearEvents : null);

        public void AddStopEvent(TweenStopEvent evt)
        {
            if (IsTweenValid())
                tween.AddStopEvent(evt);
            else
                ErrorMessage();
        }

        public void RemoveStopEvent(TweenStopEvent evt)
        {
            if (IsTweenValid())
                tween.RemoveStopEvent(evt);
            else
                ErrorMessage();
        }

        public void ClearStopEvents() => DoIfValid(tween != null ? tween.ClearStopEvents : null);

        public void AddDisposeEvent(TweenStopEvent evt)
        {
            if (IsTweenValid())
                tween.AddDisposeEvent(evt);
            else
                ErrorMessage();
        }

        public void RemoveDisposeEvent(TweenStopEvent evt)
        {
            if (IsTweenValid())
                tween.RemoveDisposeEvent(evt);
            else
                ErrorMessage();
        }

        public void ClearDisposeEvents() => DoIfValid(tween != null ? tween.ClearDisposeEvents : null);
        #endregion

        #region Utility Functions
        public Tween WithLifetime(UnityEngine.Object obj)
        {
            AddDisposeEvent(GetLifetimeDisposeEvent(obj));

            return this;
        }

        private TweenStopEvent GetLifetimeDisposeEvent(UnityEngine.Object obj)
        {
            bool disposeEvent()
            {
                if (Application.isPlaying)
                    return obj == null;
                else
                    return false;
            }

            return disposeEvent;
        }

        private readonly void DoIfValid(Action action, bool errorMessage = true)
        {
            if (IsTweenValid())
                action();
            else if (errorMessage)
                ErrorMessage();
        }

        private readonly bool IsTweenValid()
        {
            return tween != null && tween.ID == id && tween.IsValid;
        }

        private readonly void ErrorMessage()
        {
            Debug.LogError($"Invalid tween handle: {id}");
        }
        #endregion

        #region Operator Overloads
        public static bool operator==(Tween a, Tween b)
        {
            return a.tween == b.tween && a.id == b.id;
        }

        public static bool operator!=(Tween a, Tween b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is Tween tween &&
                   EqualityComparer<TweenInstance>.Default.Equals(this.tween, tween.tween) &&
                   id == tween.id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(tween, id);
        }
        #endregion
    }
}
