using System;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class TriggerParameter : Parameter<bool>
    {
        public TriggerParameter(TriggerParameterData data) : base(data)
        {
        }
    }
}
