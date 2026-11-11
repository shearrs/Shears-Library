using System;

namespace Shears
{
    /// <summary>
    /// Attribute for hinting to the inspector that a class is a wrapper of another <see cref="UnityEngine.Component"/>.<br/>
    /// Has fields for hiding the wrapped value or serializing its fields manually.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class WrapperAttribute : Attribute
    {
        /// <summary>
        /// The target type to wrap.
        /// </summary>
        public Type WrappedType { get; }

        /// <summary>
        /// Whether or not to set hide flags for the wrapped <see cref="UnityEngine.Component"/>.
        /// </summary>
        public bool HideWrappedValue { get; } = true;

        /// <summary>
        /// Where wrapped fields should be inserted in the inspector hierarchy.
        /// </summary>
        public int SerializedFieldIndex { get; set; } = -1;

        /// <summary>
        /// An optional array of field names to serialize from the wrapped type directly in the inspector.
        /// </summary>
        public string[] SerializedFields { get; set; } = Array.Empty<string>();

        /// <summary>
        /// A header for the wrapped serialized fields.
        /// </summary>
        public string Header { get; set; } = string.Empty;

        public WrapperAttribute(Type wrappedType)
        {
            WrappedType = wrappedType;
        }
    }
}
