using System;
using System.Threading;
using UnityEngine;

namespace Shears
{
    public static class SafeAwaitable
    {
        public static async Awaitable WaitForSecondsAsync(float time, CancellationToken token)
        {
            try
            {
                await Awaitable.WaitForSecondsAsync(time, token);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
