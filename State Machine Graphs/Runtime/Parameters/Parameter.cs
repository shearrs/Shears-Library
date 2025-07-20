using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public abstract class Parameter
    {
        private string name;
        public string Name { get => name; set => name = value; }

        public Parameter(ParameterData data)
        {
            name = data.Name;
        }
    }

    public abstract class Parameter<T> : Parameter
    {
        private T value;

        public T Value { get => value; set => this.value = value; }

        public Parameter(ParameterData<T> data) : base(data) 
        {
            value = data.Value;
        }
    }
}
