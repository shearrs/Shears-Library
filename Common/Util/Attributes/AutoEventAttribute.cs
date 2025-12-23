using JetBrains.Annotations;
using System;
using UnityEngine;

namespace Shears
{
    /// <summary>
    /// Fields marked with this attribute will have the specified event automatically subscribed to the specified callback method in generated OnEnable and OnDisable methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    [MeansImplicitUse]
    public class AutoEventAttribute : Attribute
    {
        private readonly string eventName;
        private readonly string callbackName;

        public string EventName => eventName;
        public string CallbackName => callbackName;

        public AutoEventAttribute(string eventName, string callbackName)
        {
            this.eventName = eventName;
            this.callbackName = callbackName;
        }
    }
}
