using System;
using UnityEngine;

namespace Shears
{
    [Serializable]
    public struct SerializableSystemType
    {
        [SerializeField] private string name;
        [SerializeField] private string assemblyQualifiedName;
        [SerializeField] private string assemblyName;
        private Type systemType;

        public string Name => name;
        public string AssemblyQualifiedName => assemblyQualifiedName;
        public string AssemblyName => assemblyName;
        public Type SystemType
        {
            get
            {
                if (systemType == null)
                    GetSystemType();

                return systemType;
            }
        }

        private void GetSystemType()
        {
            systemType = Type.GetType(assemblyQualifiedName);
        }

        public SerializableSystemType(Type type)
        {
            systemType = type;
            name = type.Name;
            assemblyQualifiedName = type.AssemblyQualifiedName;
            assemblyName = type.Assembly.FullName;
        }

        public SerializableSystemType(string assemblyQualifiedName)
        {
            Type type = Type.GetType(assemblyQualifiedName) ?? throw new Exception($"Invalid type {assemblyQualifiedName}!");

            systemType = type;
            name = type.Name;
            this.assemblyQualifiedName = assemblyQualifiedName;
            assemblyName = type.Assembly.FullName;
        }

        #region Operators
        public override bool Equals(object obj)
        {
            if (obj is not SerializableSystemType type)
                return false;

            return Equals(type);
        }

        public bool Equals(SerializableSystemType type)
        {
            return type.SystemType.Equals(SystemType);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, AssemblyQualifiedName, AssemblyName, SystemType);
        }

        public override string ToString()
        {
            return name;
        }

        public static bool operator==(SerializableSystemType a, SerializableSystemType b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (a == null || b == null)
                return false;

            return a.Equals(b);
        }

        public static bool operator!=(SerializableSystemType a, SerializableSystemType b)
        {
            return !(a == b);
        }
        #endregion
    }
}
