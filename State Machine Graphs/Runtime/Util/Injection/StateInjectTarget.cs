using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public struct StateInjectTarget
    {
        [SerializeField] private string targetID;
        [SerializeField] private SerializableSystemType targetType;
        [SerializeField] private SerializableSystemType fieldType;

        public readonly string TargetID => targetID;
        public readonly SerializableSystemType TargetType => targetType;
        public readonly SerializableSystemType FieldType => fieldType;

        public StateInjectTarget(string targetID, SerializableSystemType targetType, SerializableSystemType fieldType)
        {
            this.targetID = targetID;
            this.targetType = targetType;
            this.fieldType = fieldType;
        }
    }
}
