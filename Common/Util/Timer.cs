using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Shears
{
    /// <summary>
    /// A simple timer class.<br/><br/>
    /// 
    /// Features:<br/>
    /// - Dynamic or preset time<br/>
    /// - <see cref="SafeAwaitable"/> timer<br/>
    /// - Completion callbacks
    /// </summary>
    [Serializable]
    public class Timer
    {
        [SerializeField] private float time = 1f;
        [SerializeField] private float currentTime = 0.0f;
        [SerializeField, ReadOnly] private bool isDone = true;
        
        private CancellationTokenSource tokenSource;
        private readonly List<Action> onComplete = new();

        public float Time { get => time; set => time = value; }
        public float CurrentTime => currentTime;
        public float Percentage => CurrentTime / Time;
        public bool IsDone => isDone;

        public Timer() { }

        public Timer(float time)
        {
            this.time = time;
        }

        /// <summary>
        /// Starts the timer with the timer's default time (in seconds).<br/><br/>
        /// Default time is set in 3 ways:<br/>
        /// 1. Passed in the constructor<br/>
        /// 2. Set with <see cref="Time"/><br/>
        /// 3. Default set to 1.0
        /// </summary>
        public void Start() => Start(time);

        /// <summary>
        /// Starts the timer with a passed time (in seconds).
        /// </summary>
        /// <param name="time">The time in seconds for the timer.</param>
        public void Start(float time)
        {
            if (!isDone)
                return;

            var appToken = Application.exitCancellationToken;
            tokenSource = CancellationTokenSource.CreateLinkedTokenSource(appToken);

            RunAsync(time, tokenSource.Token);
        }

        /// <summary>
        /// Stops the timer if it is running. Does not call completion callbacks.
        /// </summary>
        public void Stop()
        {
            if (isDone)
                return;

            tokenSource?.Cancel();
            isDone = true;
        }

        /// <summary>
        /// Stops and starts the timer.
        /// </summary>
        public void Restart()
        {
            Stop();
            Start();
        }

        /// <summary>
        /// Adds a callback action for when the timer completes.<br/>
        /// These actions are not cleared and will be called every time the timer completes.
        /// </summary>
        /// <param name="action">The callback to add.</param>
        public void AddOnComplete(Action action)
        {
            if (action != null && !onComplete.Contains(action))
                onComplete.Add(action);
        }

        /// <summary>
        /// Removes a callback action from the completion list.
        /// </summary>
        /// <param name="action">The callback to remove.</param>
        public void RemoveOnComplete(Action action)
        {
            if (action != null)
                onComplete.Remove(action);
        }

        private async void RunAsync(float time, CancellationToken token)
        {
            isDone = false;
            currentTime = 0.0f;

            while (currentTime < time)
            {
                currentTime += UnityEngine.Time.deltaTime;

                await SafeAwaitable.NextFrameAsync(token);
            }

            isDone = true;

            if (token.IsCancellationRequested)
                return;

            foreach (var action in onComplete)
                action?.Invoke();
        }
    }
}
