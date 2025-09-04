using System;
using UnityEngine;

namespace Shears
{
    [Serializable]
    public struct SerializableSystemType
    {
        public static readonly SerializableSystemType Empty = new();
        
        [SerializeField] private string name;
        [SerializeField] private string assemblyQualifiedName;
        [SerializeField] private string assemblyName;
        [SerializeField] private string prettyName;
        private Type systemType;

        public readonly string Name => name;
        public readonly string AssemblyQualifiedName => assemblyQualifiedName;
        public readonly string AssemblyName => assemblyName;
        public readonly string PrettyName => prettyName;
        public Type SystemType
        {
            get
            {
                if (systemType == null)
                    GetSystemType();

                return systemType;
            }
        }

        public SerializableSystemType(Type type)
        {
            systemType = type;
            name = type.Name;
            assemblyQualifiedName = type.AssemblyQualifiedName;
            assemblyName = type.Assembly.FullName;
            prettyName = StringUtil.PascalSpace(name);
        }

        public SerializableSystemType(string assemblyQualifiedName)
        {
            Type type = Type.GetType(assemblyQualifiedName) ?? throw new Exception($"Invalid type {assemblyQualifiedName}!");

            systemType = type;
            name = type.Name;
            this.assemblyQualifiedName = assemblyQualifiedName;
            assemblyName = type.Assembly.FullName;
            prettyName = StringUtil.PascalSpace(name);
        }

        private void GetSystemType()
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName))
                systemType = null;
            else
                systemType = Type.GetType(assemblyQualifiedName);
        }

        public bool IsValid()
        {
            return SystemType != null;
        }

        #region Operators
        public override bool Equals(object obj)
        {
            if (obj is Type type)
                return SystemType.Equals(type);

            if (obj is not SerializableSystemType sType)
                return false;

            return Equals(sType);
        }

        public bool Equals(SerializableSystemType type) 
        {
            return SystemType == type.SystemType;
        }

        public bool Equals(Type type)
        {
            return SystemType == type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, AssemblyQualifiedName, AssemblyName, SystemType);
        }

        public readonly override string ToString()
        {
            return name;
        }

        public static bool operator==(SerializableSystemType a, SerializableSystemType b)
        {
            if (ReferenceEquals(a, b))
                return true;

            return a.Equals(b);
        }

        public static bool operator==(SerializableSystemType a, Type b)
        {
            return a.Equals(b);
        }

        public static bool operator!=(SerializableSystemType a, SerializableSystemType b)
        {
            return !(a == b);
        }

        public static bool operator!=(SerializableSystemType a, Type b)
        {
            return !(a == b);
        }

        public static implicit operator Type(SerializableSystemType t)
        {
            return t.SystemType;
        }

        public static implicit operator SerializableSystemType(Type t)
        {
            return new(t);
        }
        #endregion
    }
}
