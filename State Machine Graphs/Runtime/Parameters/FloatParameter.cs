using System;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class FloatParameter : Parameter<float>
    {
        public FloatParameter(FloatParameterData data) : base(data)
        {
        }
    }
}
