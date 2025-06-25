using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Shears
{
    [System.Serializable]
    public class Timer
    {
        [SerializeField] private float time = 1;
        [SerializeField, ReadOnly] private bool isDone = true;
        
        private CancellationTokenSource tokenSource;
        private readonly List<Action> onComplete = new();

        public float Time { get => time; set => time = value; }
        public bool IsDone => isDone;

        public Timer() { }

        public Timer(float time)
        {
            this.time = time;
        }

        public void Start() => Start(time);

        public void Start(float time)
        {
            if (!isDone)
                return;

            var appToken = Application.exitCancellationToken;
            tokenSource = CancellationTokenSource.CreateLinkedTokenSource(appToken);

            RunAsync(time, tokenSource.Token);
        }

        public void Stop()
        {
            if (isDone)
                return;

            tokenSource?.Cancel();
            isDone = true;
        }

        public void AddOnComplete(Action action)
        {
            if (action != null && !onComplete.Contains(action))
                onComplete.Add(action);
        }

        public void RemoveOnComplete(Action action)
        {
            if (action != null)
                onComplete.Remove(action);
        }

        private async void RunAsync(float time, CancellationToken token)
        {
            isDone = false;

            await SafeAwaitable.WaitForSecondsAsync(time, token);

            isDone = true;

            foreach (var action in onComplete)
                action?.Invoke();
        }
    }
}
