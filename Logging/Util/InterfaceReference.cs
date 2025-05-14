using UnityEngine;
using System;
using Object = UnityEngine.Object;

namespace InternProject.Logging
{
    /// <summary>
    /// A wrapper class for serializing a <see cref="TObject"/> class type that implements a <see cref="TInterface"/> interface type in the Unity inspector.
    /// </summary>
    /// <typeparam name="TInterface">The type of interface to have implemented.</typeparam>
    /// <typeparam name="TObject">The type of object to serialize.</typeparam>
    [Serializable]
    public class InterfaceReference<TInterface, TObject> where TObject : Object where TInterface : class
    {
        [SerializeField, HideInInspector] private TObject objectValue;

        /// <summary>
        /// The <see cref="TInterface"/> value held by the wrapper.
        /// </summary>
        public TInterface Value
        {
            get => objectValue switch
            {
                null => null,
                TInterface @interface => @interface,
                _ => null
            };
            set => objectValue = value switch
            {
                null => null,
                TObject newValue => newValue,
                _ => throw new ArgumentException($"{value} needs to be of type {typeof(TObject)}.")
            };
        }

        /// <summary>
        /// The <see cref="TObject"/> value held by the wrapper.
        /// </summary>
        public TObject ObjectValue
        {
            get => objectValue;
            set => objectValue = value;
        }

        public InterfaceReference() { }
        public InterfaceReference(TObject obj) => objectValue = obj;
        public InterfaceReference(TInterface value) => objectValue = value as TObject;
    }

    /// <summary>
    /// A wrapper class for serializing an interface in the Unity inspector.
    /// </summary>
    /// <typeparam name="TInterface">The interface to have implemented.</typeparam>
    [Serializable]
    public class InterfaceReference<TInterface> : InterfaceReference<TInterface, Object> where TInterface : class
    {
    }
}
