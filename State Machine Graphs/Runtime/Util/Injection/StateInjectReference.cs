using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public partial class StateInjectReference
    {
        [SerializeField] private List<string> targetIDs = new();
        [SerializeField] private SerializableSystemType fieldType;
        [SerializeField] private Object value;

        public IReadOnlyList<string> TargetIDs => targetIDs;
        public SerializableSystemType FieldType => fieldType;
        public Object Value => value;

        public StateInjectReference(SerializableSystemType fieldType)
        {
            this.fieldType = fieldType;
        }

        public void AddTarget(string id)
        {
            if (!targetIDs.Contains(id))
                targetIDs.Add(id);
        }

        public void RemoveTarget(string id)
        {
            if (targetIDs.Contains(id))
                targetIDs.Remove(id);
        }
    }
}
