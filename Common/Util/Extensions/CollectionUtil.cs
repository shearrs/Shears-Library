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

        public static string ToCollectionString<T>(this IReadOnlyCollection<T> collection, Func<T, string> toString = null, string separator = ", ")
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
    }
}
