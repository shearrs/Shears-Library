using UnityEngine;

namespace InternProject.StateMachines
{
    /// <summary>
    /// A base class for a comparison between a parameter and a value.
    /// </summary>
    [System.Serializable]
    public abstract class ParameterComparison
    {
        [SerializeField]
        private string _parameterID;

        internal string ParameterID => _parameterID;
        public abstract Parameter Parameter { get; internal set;}

        public bool Evaluate()
        {
            if (Parameter == null)
                return false;

            return EvaluateInternal();
        }

        public abstract bool EvaluateInternal();
    }

    /// <summary>
    /// A typed base class for a comparison between a parameter and a value.
    /// </summary>
    /// <typeparam name="T">The type of the parameter and compare value.</typeparam>
    [System.Serializable]
    public abstract class ParameterComparison<T> : ParameterComparison
    {
        protected Parameter<T> _parameter;

        [SerializeField]
        protected T _compareValue;

        public override Parameter Parameter { get => _parameter; internal set => _parameter = value as Parameter<T>; }
    }
}
