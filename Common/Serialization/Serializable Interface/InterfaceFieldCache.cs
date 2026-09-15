using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Shears
{
    /// <summary>
    /// Static cache to map types to their serialized interface fields and limit repeated reflection.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class InterfaceFieldCache
    {
        /// <summary>
        /// A mapping of types to their serialized interface fields.
        /// </summary>
        private static readonly Dictionary<Type, FieldInfo[]> interfaceFieldCache;

        /// <summary>
        /// Initialize the <see cref="InterfaceFieldCache"/>.
        /// </summary>
        static InterfaceFieldCache()
        {
            interfaceFieldCache = new();
        }

        /// <summary>
        /// Get the cached serialized interface fields for a given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to get fields for.</param>
        /// <returns>The cached serialized interface fields for the passed <see cref="Type"/>.</returns>
        public static FieldInfo[] GetCachedInterfaceFields(Type type)
        {
            if (!interfaceFieldCache.TryGetValue(type, out var fields))
            {
                var allFields = type.GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                );

                var filteredList = new List<FieldInfo>();

                foreach (var field in allFields)
                {
                    if (
                        field.FieldType.IsInterface && field.IsDefined(typeof(SerializeField), true)
                    )
                        filteredList.Add(field);
                }

                fields = filteredList.ToArray();
                interfaceFieldCache[type] = fields;
            }

            return fields;
        }
    }
}
