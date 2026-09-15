using UnityEngine;

namespace Shears
{
    /// <summary>
    /// A serialized wrapper for an interface field. Stored in an <see cref="InterfaceDictionary"/>.
    /// </summary>
    [System.Serializable]
    public class InterfaceEntry
    {
        /// <inheritdoc cref="UnityObject"/>
        [SerializeField]
        [Tooltip("The UnityEngine.Object value.")]
        private Object unityObject;

        /// <inheritdoc cref="RawObject"/>
        [SerializeReference]
        [Tooltip("The raw C# object value.")]
        private object rawObject;

#if UNITY_EDITOR
        /// <inheritdoc cref="FieldName"/>
        [SerializeField]
        [Tooltip("The name of the field this targets.")]
        private string fieldName;

        /// <inheritdoc cref="FieldType"/>
        [SerializeField]
        [Tooltip("The type of the field this targets.")]
        private SerializableType fieldType;

        /// <summary>
        /// The name of the field this targets.
        /// </summary>
        public string FieldName
        {
            get => fieldName;
            set => fieldName = value;
        }

        /// <summary>
        /// The type of the field this targets.
        /// </summary>
        public SerializableType FieldType
        {
            get => fieldType;
            set => fieldType = value;
        }
#endif

        /// <summary>
        /// The <see cref="Object"/> value.
        /// </summary>
        public Object UnityObject
        {
            get => unityObject;
            set => unityObject = value;
        }

        /// <summary>
        /// The raw C# <see cref="object"/> value.
        /// </summary>
        public object RawObject
        {
            get => rawObject;
            set => rawObject = value;
        }

        /// <summary>
        /// Create a new <see cref="InterfaceEntry"/>.
        /// </summary>
        /// <param name="unityObject">The <see cref="Object"/> value.</param>
        /// <param name="rawObject">The raw C# <see cref="object"/> value.</param>
        public InterfaceEntry(Object unityObject, object rawObject)
        {
            this.unityObject = unityObject;
            this.rawObject = rawObject;
        }
    }
}
