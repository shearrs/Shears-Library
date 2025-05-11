using UnityEngine;

namespace InternProject.StateMachines
{
    [System.Serializable]
    public abstract class Parameter
    {
        [SerializeField]
        private string _name;

        [SerializeField, HideInInspector]
        private string _id;

        public string Name { get => _name; set => _name = value; }
        public string ID => _id;

        public Parameter(string name)
        {
            _name = name;
            _id = System.Guid.NewGuid().ToString();
        }
    }

    [System.Serializable]
    public abstract class Parameter<T> : Parameter
    {
        [SerializeField]
        private T _value;

        public T Value { get => _value; set => _value = value; }

        public Parameter(string name) : base(name) { }
    }
}
