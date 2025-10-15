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
        private readonly List<Condition> conditions = new();

        public IReadOnlyList<Condition> Conditions => conditions;

        public readonly struct Condition
        {
            private readonly string conditionName;
            private readonly object compareValue;

            public readonly string ConditionName => conditionName;
            public readonly object CompareValue => compareValue;
            
            public Condition(string conditionName, object compareValue)
            {
                this.conditionName = conditionName;
                this.compareValue = compareValue;
            }
        }

        /// <summary>
        /// Shows the field if the condition with <see cref="conditionName"/> has a value of <c>true</c>.
        /// </summary>
        /// <param name="conditionName">The name of the condition to evaluate.</param>
        public ShowIfAttribute(params string[] conditions)
        {
            foreach (var condition in conditions)
                this.conditions.Add(new(condition, true));
        }

        /// <summary>
        /// Shows the field if the condition with <see cref="conditionName"/> is equal to <see cref="compareValue"/>.
        /// </summary>
        /// <param name="condition"></param>
        public ShowIfAttribute(string conditionName, object compareValue)
        {
            conditions.Add(new(conditionName, compareValue));
        }

        public ShowIfAttribute(string conditionName1, object compareValue1, string conditionName2, object compareValue2)
        {
            conditions.Add(new(conditionName1, compareValue1));
            conditions.Add(new(conditionName2, compareValue2));
        }
    }
}
