using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    public abstract class CustomInspector : VisualElement
    {
        public abstract Type TargetType { get; }
        public abstract bool Fallback { get; }

        public abstract VisualElement Create(SerializedObject serializedObject);
    }
}
