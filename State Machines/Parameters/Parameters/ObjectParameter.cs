using UnityEngine;

namespace Shears.StateMachines
{
    public class ObjectParameter : Parameter<Object>
    {
        public ObjectParameter(string name) : base(name)
        {
        }

        public ObjectParameter(string name, Object value) : base(name, value)
        {
        }
    }
}
