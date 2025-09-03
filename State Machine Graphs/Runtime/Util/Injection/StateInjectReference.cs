using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class StateInjectReference
    {
        [SerializeField] private string parentGraphID;
        [SerializeField] private string graphID;
        [SerializeField] private List<string> targetIDs = new();
        [SerializeField] private SerializableSystemType fieldType;
        [SerializeField] private Object value;

        public string ParentGraphID => parentGraphID;
        public string GraphID { get => graphID; set => graphID = value; }
        public IReadOnlyList<string> TargetIDs => targetIDs;
        public SerializableSystemType FieldType => fieldType;
        public Object Value => value;

        public StateInjectReference(string parentGraphID, SerializableSystemType fieldType)
        {
            this.parentGraphID = parentGraphID;
            graphID = parentGraphID;
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
