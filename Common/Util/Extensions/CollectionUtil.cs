using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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

        public static T Random<T>(this List<T> list)
        {
            if (list.Count == 0)
                return default;
            else
                return list[UnityEngine.Random.Range(0, list.Count)];
        }
    }
}
