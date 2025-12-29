using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    public static class TimerPool
    {
        private static readonly List<Timer> pool = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            pool.Clear();
        }

        public static Timer Get()
        {
            if (pool.Count > 0)
            {
                var timer = pool[^1];
                pool.RemoveAt(pool.Count - 1);

                return timer;
            }
            else
                return new Timer();
        }

        public static void Release(Timer timer)
        {
            timer.Stop();
            timer.ClearOnCompletes();

            pool.Add(timer);
        }
    }
}
