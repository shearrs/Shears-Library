using System;

namespace Shears.Tweens
{
    public interface ITween
    {
        public bool IsValid { get; }
        public float Duration { get; }
        public bool IsPlaying { get; }
        public bool Paused { get; }
        public int Loops { get; }

        public void Play();
        public void Stop();
        public void Pause();
        public void Dispose();

        public void AddOnComplete(Action onComplete);
        public void RemoveOnComplete(Action onComplete);

        public void AddStopEvent(TweenStopEvent evt);
        public void RemoveStopEvent(TweenStopEvent evt);
        public void ClearStopEvents();

        public void AddDisposeEvent(TweenStopEvent evt);
        public void RemoveDisposeEvent(TweenStopEvent evt);
        public void ClearDisposeEvents();
    }
}