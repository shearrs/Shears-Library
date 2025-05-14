using System;
using UnityEngine;

namespace InternProject.Logging
{
    /// <summary>
    /// An attribute for serializing a field only when a condition is met.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ShowIfAttribute : PropertyAttribute
    {
        private readonly string _conditionName;
        private readonly object _compareValue;

        /// <summary>
        /// The name of the conditional variable.
        /// </summary>
        public string ConditionName => _conditionName;

        /// <summary>
        /// The value to compare the conditional variable to.
        /// </summary>
        public object CompareValue => _compareValue;

        /// <summary>
        /// Shows the field if the condition with <see cref="conditionName"/> has a value of <c>true</c>
        /// </summary>
        /// <param name="conditionName">The name of the condition to evaluate.</param>
        public ShowIfAttribute(string conditionName)
        {
            _conditionName = conditionName;
            _compareValue = true;
        }

        /// <summary>
        /// Shows the field if the condition with <see cref="conditionName"/> is equal to <see cref="compareValue"/>.
        /// </summary>
        /// <param name="conditionName"></param>
        /// <param name="compareValue"></param>
        public ShowIfAttribute(string conditionName, object compareValue)
        {
            _conditionName = conditionName;
            _compareValue = compareValue;
        }
    }
}
