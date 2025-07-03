using UnityEngine;

namespace Shears.StateMachines
{
    [System.Serializable]
    public abstract class Parameter
    {
        [SerializeField]
        private string name;

        [SerializeField, HideInInspector]
        private string id;

        public string Name { get => name; set => name = value; }
        public string ID => id;

        public Parameter(string name)
        {
            this.name = name;
            id = System.Guid.NewGuid().ToString();
        }
    }

    [System.Serializable]
    public abstract class Parameter<T> : Parameter
    {
        [SerializeField]
        private T value;

        public T Value { get => value; set => this.value = value; }

        public Parameter(string name) : base(name) { }
    }
}
