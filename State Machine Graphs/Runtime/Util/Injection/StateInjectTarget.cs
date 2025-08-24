using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public struct StateInjectTarget
    {
        [SerializeField] private string targetID;
        [SerializeField] private string fieldName;
        [SerializeField] private SerializableSystemType fieldType;

        public readonly string TargetID => targetID;
        public readonly string FieldName => fieldName;
        public readonly SerializableSystemType FieldType => fieldType;

        public StateInjectTarget(string targetID, string fieldName, SerializableSystemType fieldType)
        {
            this.targetID = targetID;
            this.fieldName = fieldName;
            this.fieldType = fieldType;
        }
    }
}
