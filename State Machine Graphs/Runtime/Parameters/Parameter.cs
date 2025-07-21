using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public abstract class Parameter
    {
        [SerializeField] private string name;
        public string Name { get => name; set => name = value; }

        public Parameter(ParameterData data)
        {
            name = data.Name;
        }
    }

    [System.Serializable]
    public abstract class Parameter<T> : Parameter
    {
        [SerializeField] private T value;

        public T Value { get => value; set => this.value = value; }

        public Parameter(ParameterData<T> data) : base(data) 
        {
            value = data.Value;
        }
    }
}
