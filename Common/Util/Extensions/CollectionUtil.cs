using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Pool;

namespace Shears
{
    public static class CollectionUtil
    {
        private static readonly StringBuilder stringBuilder = new();

        public static string ToCollectionString<T>(
            this IReadOnlyCollection<T> collection,
            Func<T, string> toString = null,
            string separator = ", "
        )
        {
            stringBuilder.Clear();

            for (int i = 0; i < collection.Count; i++)
            {
                if (toString != null)
                    stringBuilder.Append(toString(collection.ElementAt(i)));
                else
                {
                    var element = collection.ElementAt(i);
                    string value = (element != null) ? element.ToString() : "NULL";

                    stringBuilder.Append(value);
                }

                if (i < collection.Count - 1)
                    stringBuilder.Append(separator);
            }

            return stringBuilder.ToString();
        }

        public static void Remove<T>(this List<T> list, Ref<T> refVariable)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(list[i], refVariable.Value))
                {
                    list.RemoveAt(i);
                    return;
                }
            }
        }

        public static void Remove<T>(this List<Ref<T>> list, T var)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(list[i].Value, var))
                {
                    list.RemoveAt(i);
                    return;
                }
            }
        }

        public static int IndexOf<T>(this List<Ref<T>> list, T var)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(list[i].Value, var))
                    return i;
            }

            return -1;
        }

        public static int IndexOf<T>(this IReadOnlyList<T> list, T var)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(list[i], var))
                    return i;
            }

            return -1;
        }

        public static T Random<T>(this List<T> list)
        {
            if (list.Count == 0)
                return default;
            else
                return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static T Random<T>(this T[] array)
        {
            if (array.Length == 0)
                return default;
            else
                return array[UnityEngine.Random.Range(0, array.Length)];
        }

        public static void GetPooled<T>(out List<T> list)
        {
            list = ListPool<T>.Get();
        }

        public static void ReleasePooled<T>(List<T> list)
        {
            ListPool<T>.Release(list);
        }
    }
}
