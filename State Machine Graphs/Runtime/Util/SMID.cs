using System;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public readonly struct SMID
    {
        public static readonly SMID Empty;

        private readonly Guid id;

        private SMID(Guid id)
        {
            this.id = id;
        }

        public static SMID Create()
        {
            return new(Guid.NewGuid());
        }

        public override bool Equals(object obj)
        {
            return obj is SMID sMID &&
                   id.Equals(sMID.id);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
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
