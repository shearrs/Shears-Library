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
        [SerializeField] private T min;
        [SerializeField] private T max;

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

        /// <summary>
        /// Returns a random value within the range. Only supported for int and float types.
        /// </summary>
        /// <returns>A random int or float value</returns>
        /// <exception cref="NotSupportedException"></exception>
        public readonly T Random()
        {
            if (typeof(T) == typeof(int))
            {
                int minValue = Convert.ToInt32(min);
                int maxValue = Convert.ToInt32(max);

                return (T)(object)UnityEngine.Random.Range(minValue, maxValue + 1);
            }
            else if (typeof(T) == typeof(float))
            {
                float minValue = Convert.ToSingle(min);
                float maxValue = Convert.ToSingle(max);

                return (T)(object)UnityEngine.Random.Range(minValue, maxValue);
            }
            else
                throw new NotSupportedException("Random method is only supported for int and float types.");
        }
    }
}
