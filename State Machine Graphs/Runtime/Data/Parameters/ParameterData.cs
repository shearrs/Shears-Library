using Shears.GraphViews;
using System;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public abstract class ParameterData : GraphElementData
    {
        [SerializeField] private string name;

        public string Name { get => name; set => name = value; }

        protected abstract string DefaultName { get; }

        public event Action Selected;
        public event Action Deselected;

        public abstract ParameterComparisonData CreateComparisonData();
        public abstract Parameter CreateParameter();

        public override void Select()
        {
            Selected?.Invoke();
        }

        public override void Deselect()
        {
            Deselected?.Invoke();
        }
    }

    [Serializable]
    public abstract class ParameterData<T> : ParameterData
    {
        [SerializeField] private T value;

        public T Value { get => value; set => this.value = value; }

        public ParameterData()
        {
            Name = DefaultName;
        }

        public override ParameterComparisonData CreateComparisonData() => CreateTypedComparisonData();
        protected abstract ParameterComparisonData<T> CreateTypedComparisonData();

        public override Parameter CreateParameter() => CreateTypedParameter();
        public abstract Parameter<T> CreateTypedParameter();
    }
}
