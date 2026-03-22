using System;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public struct SMID
    {
        public static readonly SMID Empty;

        [SerializeField]
        private string id;
        
        private SMID(Guid id)
        {
            this.id = id.ToString();
        }

        public static SMID Create()
        {
            return new(Guid.NewGuid());
        }

        public readonly override bool Equals(object obj)
        {
            return obj is SMID sMID &&
                   id.Equals(sMID.id);
        }

        public readonly override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public readonly override string ToString()
        {
            return id;
        }

        public static bool operator==(SMID a, SMID b)
        {
            return a.id == b.id;
        }

        public static bool operator!=(SMID a, SMID b)
        {
            return a.id != b.id;
        }
    }
}
