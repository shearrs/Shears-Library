using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class StateInjectReference // make this a map to each individual field in each individual type instead of one type and reference combo
    {
        [SerializeField] private SerializableSystemType parentType;
        [SerializeField] private SerializableSystemType type;
        [SerializeField] private Object reference;

        public SerializableSystemType Type => type;
        public Object Reference => reference;

        public StateInjectReference(SerializableSystemType type)
        {
            this.type = type;
        }
    }
}
