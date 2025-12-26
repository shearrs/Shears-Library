using System;
using UnityEngine;

namespace Shears
{
    /// <summary>
    /// A struct representing a range between two comparable values.
    /// </summary>
    /// <typeparam name="T">The comparable value type.</typeparam>
    [Serializable]
    public struct Range<T> : IComparable where T : IComparable
    {
        [SerializeField, Delayed] private T min;
        [SerializeField, Delayed] private T max;

        public readonly T Min => min;
        public readonly T Max => max;

        public Range(T min, T max)
        {
            if (min.CompareTo(max) > 0)
                max = min;

            this.min = min;
            this.max = max;
        }

        /// <summary>
        /// Compares the max values of two ranges.
        /// </summary>
        /// <param name="obj">The other range to compare to.</param>
        /// <returns>The CompareTo value of both range's max values.</returns>
        /// <exception cref="ArgumentException"></exception>
        public int CompareTo(object obj)
        {
            if (obj is not Range<T> other)
                throw new ArgumentException("Object is not a Range of the same type.");

            int comparison = max.CompareTo(other.max);

            return comparison;
        }
    }

    public static class RangeUtil
    {
        /// <summary>
        /// Returns a random value within the range.
        /// </summary>
        /// <returns>A random int value.</returns>
        public static int Random(this Range<int> range)
        {
            return UnityEngine.Random.Range(range.Min, range.Max + 1);
        }

        /// <summary>
        /// Returns a random value within the range.
        /// </summary>
        /// <returns>A random float value.</returns>
        public static float Random(this Range<float> range)
        {
            return UnityEngine.Random.Range(range.Min, range.Max);
        }

        public static float Midpoint(this Range<float> range)
        {
            return 0.5f * (range.Min + range.Max);
        }

        /// <summary>
        /// Interpolates between the minimum and maximum range values by t.
        /// </summary>
        /// <param name="range">The range to interpolate between.</param>
        /// <param name="t">The interpolation parameter.</param>
        /// <returns>The interpolated value between the minimum and maximum range values.</returns>
        public static float Lerp(this Range<float> range, float t, bool invertMinMax = false)
        {
            float min = invertMinMax ? range.Max : range.Min;
            float max = invertMinMax ? range.Min : range.Max;

            return Mathf.Lerp(min, max, t);
        }

        /// <summary>
        /// Clamps the passed value between the range's minimum and maximum values.
        /// </summary>
        /// <param name="range">The range to clamp between.</param>
        /// <param name="value">The value to clamp.</param>
        /// <returns>The clamped value.</returns>
        public static float Clamp(this Range<float> range, float value)
        {
            return Mathf.Clamp(value, range.Min, range.Max);
        }
    }
}
