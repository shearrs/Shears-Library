using UnityEngine;

namespace Shears.StateMachines
{
    /// <summary>
    /// A base class for a comparison between a parameter and a value.
    /// </summary>
    [System.Serializable]
    public abstract class ParameterComparison
    {
        [SerializeField]
        private string parameterID;

        internal string ParameterID => parameterID;
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
        protected Parameter<T> parameter;

        [SerializeField]
        protected T compareValue;

        public override Parameter Parameter { get => parameter; internal set => parameter = value as Parameter<T>; }
    }
}
