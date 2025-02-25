using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shears
{
    [Serializable]
    public class InterfaceReference<TInterface, TObject> where TObject : Object where TInterface : class
    {
        [SerializeField, HideInInspector] private TObject objectValue;

        public TInterface Value
        {
            get => objectValue switch
            {
                null => null,
                TInterface @interface => @interface,
                _ => throw new InvalidOperationException($"{objectValue} needs to implement interface {typeof(TInterface).Name}.")
            };
            set => objectValue = value switch
            {
                null => null,
                TObject newValue => newValue,
                _ => throw new ArgumentException($"{value} needs to be of type {typeof(TObject)}.")
            };
        }

        public TObject ObjectValue
        {
            get => objectValue;
            set => objectValue = value;
        }

        public InterfaceReference() { }
        public InterfaceReference(TObject obj) => objectValue = obj;
        public InterfaceReference(TInterface value) => objectValue = value as TObject;
    }

    [Serializable]
    public class InterfaceReference<TInterface> : InterfaceReference<TInterface, Object> where TInterface : class
    {
    }
}
