using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Shears
{
    /// <summary>
    /// A serializable wrapper of <see cref="Type"/>.
    /// </summary>
    [MovedFrom(true, "Shears", "Assembly-CSharp", "SerializableSystemType")]
    [Serializable]
    public class SerializableType
    {
        public static readonly SerializableType Empty = new(null);

        [SerializeField]
        private string name;

        [SerializeField]
        private string assemblyQualifiedName;

        private Type systemType;

        public string Name => name;
        public string AssemblyQualifiedName => assemblyQualifiedName;
        public string PrettyName => Name.PascalSpace();
        public Type SystemType
        {
            get
            {
                if (systemType == null || systemType.AssemblyQualifiedName != assemblyQualifiedName)
                    GetSystemType();

                return systemType;
            }
        }

        public SerializableType(Type type)
        {
            if (type == null)
            {
                systemType = null;
                name = string.Empty;
                assemblyQualifiedName = string.Empty;

                return;
            }

            systemType = type;
            name = type.Name;
            assemblyQualifiedName = type.AssemblyQualifiedName;
        }

        private void GetSystemType()
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName))
                systemType = null;
            else
                systemType = Type.GetType(assemblyQualifiedName);
        }

        /// <summary>
        /// Returns whether this <see cref="SerializableType"/> is valid (i.e. wraps a non-null <see cref="Type"/>).
        /// </summary>
        /// <returns>Whether or not this type is valid.</returns>
        public bool IsValid()
        {
            return SystemType != null;
        }

        #region Operators
        public override bool Equals(object obj)
        {
            if (obj is Type type)
                return SystemType.Equals(type);

            if (obj is not SerializableType sType)
                return false;

            return Equals(sType);
        }

        public bool Equals(SerializableType type)
        {
            return SystemType == type.SystemType;
        }

        public bool Equals(Type type)
        {
            return SystemType == type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, AssemblyQualifiedName, SystemType);
        }

        public override string ToString()
        {
            return name;
        }

        public static bool operator ==(SerializableType a, SerializableType b)
        {
            if (ReferenceEquals(a, b))
                return true;

            return a.Equals(b);
        }

        public static bool operator ==(SerializableType a, Type b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SerializableType a, SerializableType b)
        {
            return !(a == b);
        }

        public static bool operator !=(SerializableType a, Type b)
        {
            return !(a == b);
        }

        public static implicit operator Type(SerializableType t)
        {
            return t.SystemType;
        }

        public static implicit operator SerializableType(Type t)
        {
            return new(t);
        }
        #endregion
    }
}
