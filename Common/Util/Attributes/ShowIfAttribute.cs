using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    /// <summary>
    /// An attribute for serializing a field only when a condition is met.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ShowIfAttribute : PropertyAttribute
    {
        private readonly Dictionary<string, object> conditions = new();

        public IReadOnlyDictionary<string, object> Conditions => conditions;

        /// <summary>
        /// Shows the field if the condition with <see cref="conditionName"/> has a value of <c>true</c>.
        /// </summary>
        /// <param name="conditionName">The name of the condition to evaluate.</param>
        public ShowIfAttribute(params string[] conditions)
        {
            foreach (var condition in conditions)
                this.conditions[condition] = true;
        }

        /// <summary>
        /// Shows the field if the condition with <see cref="conditionName"/> is equal to <see cref="compareValue"/>.
        /// </summary>
        /// <param name="condition"></param>
        public ShowIfAttribute(params (string conditionName, object compareValue)[] conditions)
        {
            foreach (var (conditionName, compareValue) in conditions)
                this.conditions[conditionName] = compareValue;
        }
    }
}
