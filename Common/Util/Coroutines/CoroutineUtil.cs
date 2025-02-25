using Shears.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    public static class CoroutineUtil
    {
        static readonly Dictionary<float, WaitForSeconds> waitForSeconds = new();
        static readonly Dictionary<float, WaitForSecondsRealtime> waitForSecondsRealtime = new();

        public static WaitForSeconds WaitForSeconds(float seconds)
        {
            if (waitForSeconds.TryGetValue(seconds, out WaitForSeconds wait))
                return wait;

            wait = new(seconds);
            waitForSeconds.Add(seconds, wait);

            return wait;
        }

        public static WaitForSecondsRealtime WaitForSecondsRealtime(float seconds)
        {
            if (waitForSecondsRealtime.TryGetValue(seconds, out WaitForSecondsRealtime wait))
                return wait;

            wait = new(seconds);
            waitForSecondsRealtime.Add(seconds, wait);

            return wait;
        }

        private static IEnumerator IEDoAfter(Coroutine waitRoutine, Action doAfter)
        {
            yield return waitRoutine;

            doAfter?.Invoke();
        }
    }
}
