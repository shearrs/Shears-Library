using System;
using System.Collections.Generic;
using Codice.Client.BaseCommands;
using UnityEngine;

namespace Shears
{
    public static class MathUtil
    {
        public static float MinSelector<T>(IReadOnlyList<T> items, Func<T, float> selector)
        {
            if (items.Count == 0)
                return -1;

            float min = selector(items[0]);

            for (int i = 1; i < items.Count; i++)
            {
                float currentMin = selector(items[i]);

                if (currentMin < min)
                    min = currentMin;
            }

            return min;
        }

        public static float MaxSelector<T>(IReadOnlyList<T> items, Func<T, float> selector)
        {
            if (items.Count == 0)
                return -1;

            float max = selector(items[0]);

            for (int i = 1; i < items.Count; i++)
            {
                float currentMax = selector(items[i]);

                if (currentMax > max)
                    max = currentMax;
            }

            return max;
        }

        public static void MinMax(out float min, out float max, params float[] values)
        {
            min = Mathf.Min(values);
            max = Mathf.Max(values);
        }

        public static void MinMax<T>(
            out float min,
            out float max,
            IReadOnlyList<T> list,
            Func<T, float> minSelector,
            Func<T, float> maxSelector
        )
        {
            if (list.Count == 0)
            {
                min = -1;
                max = -1;

                return;
            }

            min = minSelector(list[0]);
            max = maxSelector(list[0]);

            for (int i = 1; i < list.Count; i++)
            {
                float minCandidate = minSelector(list[i]);
                float maxCandidate = maxSelector(list[i]);

                if (minCandidate < min)
                    min = minCandidate;

                if (maxCandidate > max)
                    max = maxCandidate;
            }
        }
    }
}
