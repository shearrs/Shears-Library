using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class StateInjectReference
    {
        [SerializeField, ReadOnly] private string stateID;
        [SerializeField, ReadOnly] private SerializableSystemType type;
        [SerializeField] private Object reference;

        public string StateID => stateID;
        public SerializableSystemType Type => type;
        public Object Reference => reference;

        public StateInjectReference(string stateID, SerializableSystemType type)
        {
            this.stateID = stateID;
            this.type = type;
        }
    }
}
