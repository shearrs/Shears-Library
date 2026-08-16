using System;
using System.Threading;
using UnityEngine;

namespace Shears
{
    public static class SafeAwaitable
    {
        public static async Awaitable NextFrameAsync(CancellationToken? token = null)
        {
            try
            {
                var tokenValue = token ?? Application.exitCancellationToken;

                await Awaitable.NextFrameAsync(tokenValue);
            }
            catch (OperationCanceledException) { }
        }

        public static async Awaitable WaitForSecondsAsync(
            float time,
            CancellationToken? token = null
        )
        {
            try
            {
                var tokenValue = token ?? Application.exitCancellationToken;

                await Awaitable.WaitForSecondsAsync(time, tokenValue);
            }
            catch (OperationCanceledException) { }
        }
    }
}
