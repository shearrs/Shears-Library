using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class StateInjectReference
    {
        [SerializeField] private string graphID;
        [SerializeField] private List<string> targetIDs = new();
        [SerializeField] private SerializableSystemType fieldType;
        [SerializeField] private Object value;

        public string GraphID => graphID;
        public IReadOnlyList<string> TargetIDs => targetIDs;
        public SerializableSystemType FieldType => fieldType;
        public Object Value => value;

        public StateInjectReference(string graphID, SerializableSystemType fieldType)
        {
            this.graphID = graphID;
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
