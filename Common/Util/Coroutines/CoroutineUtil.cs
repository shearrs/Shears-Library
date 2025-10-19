using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    public static class CoroutineUtil
    {
        private static readonly Dictionary<float, WaitForSeconds> waitForSeconds = new();
        private static readonly Dictionary<float, WaitForSecondsRealtime> waitForSecondsRealtime = new();
        private static readonly WaitForEndOfFrame waitForEndOfFrame = new();

        public static WaitForEndOfFrame WaitForEndOfFrame => waitForEndOfFrame;

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

        public static Coroutine DoAfter(Action action, float time)
        {
            return CoroutineRunner.Start(IEDoAfter(action, time));
        }

        private static IEnumerator IEDoAfter(Action action, float time)
        {
            yield return WaitForSeconds(time);

            action?.Invoke();
        }
    }
}
