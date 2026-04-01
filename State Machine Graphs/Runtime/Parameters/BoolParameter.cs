using System;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class BoolParameter : Parameter<bool>
    {
        public BoolParameter(BoolParameterData data) : base(data)
        {
        }
    }
}
