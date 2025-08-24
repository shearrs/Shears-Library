using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public partial class StateInjectReference
    {
        [SerializeField] private List<StateInjectTarget> targets = new();
        [SerializeField] private SerializableSystemType fieldType;
        [SerializeField] private Object value;

        public IReadOnlyList<StateInjectTarget> Targets => targets;
        public SerializableSystemType FieldType => fieldType;
        public Object Value => value;

        public StateInjectReference(SerializableSystemType fieldType)
        {
            this.fieldType = fieldType;
        }

        public void AddTarget(string targetID, SerializableSystemType targetType)
            => AddTarget(new StateInjectTarget(targetID, targetType, fieldType));

        public void AddTarget(StateInjectTarget target)
        {
            if (targets.Exists(t => t.TargetID == target.TargetID))
                return;

            targets.Add(target);
        }
    }
}
