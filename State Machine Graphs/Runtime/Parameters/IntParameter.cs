using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class IntParameter : Parameter<int>
    {
        public IntParameter(IntParameterData data) : base(data)
        {
        }
    }
}
