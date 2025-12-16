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
        private static readonly WaitForFixedUpdate waitForFixedUpdate = new();

        public static WaitForEndOfFrame WaitForEndOfFrame => waitForEndOfFrame;
        public static WaitForFixedUpdate WaitForFixedUpdate => waitForFixedUpdate;

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

        public static Coroutine DoAfter(Action action, float time, Component lifetime = null)
        {
            bool hasLifetime = lifetime != null;

            return CoroutineRunner.Start(IEDoAfter(action, time, lifetime, hasLifetime));
        }

        private static IEnumerator IEDoAfter(Action action, float time, Component lifetime, bool hasLifetime)
        {
            yield return WaitForSeconds(time);

            if (hasLifetime && lifetime == null)
                yield break;

            action?.Invoke();
        }

        public static Coroutine DoDeferred(Action action)
        {
            return CoroutineRunner.Start(IEDoDeferred(action));
        }

        private static IEnumerator IEDoDeferred(Action action)
        {
            yield return WaitForEndOfFrame;
            
            action?.Invoke();
        }
    }
}
