using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shears
{
    /// <summary>
    /// The base class for <see cref="InterfaceReference{TInterface}"/>.
    /// </summary>
    /// <typeparam name="TInterface">The interface to implement.</typeparam>
    /// <typeparam name="TObject">The <see cref="Object"/> type that implements the interface.</typeparam>
    [Serializable]
    public class InterfaceReference<TInterface, TObject> where TObject : Object where TInterface : class
    {
        [SerializeField, HideInInspector] private TObject objectValue;

        /// <summary>
        /// The wrapped <see cref="TInterface"/> value.
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
        /// The wrapped <see cref="TObject"/> value.
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
    /// A serializable reference to an interface implemented by a Unity <see cref="Object"/>.
    /// </summary>
    /// <typeparam name="TInterface">The interface to implement.</typeparam>
    [Serializable]
    public class InterfaceReference<TInterface> : InterfaceReference<TInterface, Object> where TInterface : class
    {
    }
}
