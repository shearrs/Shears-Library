using System;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public abstract class Parameter
    {
        [SerializeField] private string name;
        private Guid id;

        public string Name { get => name; set => name = value; }
        internal Guid ID { get => id; set => id = value; }

        public Parameter(ParameterData data)
        {
            name = data.Name;
        }
    }

    [Serializable]
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
