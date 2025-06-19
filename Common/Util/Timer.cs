using System.Threading;
using UnityEngine;

namespace Shears
{
    [System.Serializable]
    public class Timer
    {
        [SerializeField, ReadOnly] private bool isDone = true;
        
        private CancellationTokenSource tokenSource;

        public bool IsDone => isDone;

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

        private async void RunAsync(float time, CancellationToken token)
        {
            isDone = false;

            await SafeAwaitable.WaitForSecondsAsync(time, token);

            isDone = true;
        }
    }
}
