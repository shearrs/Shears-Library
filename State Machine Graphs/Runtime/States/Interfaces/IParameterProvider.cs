using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public interface IParameterProvider
    {
        public T GetParameter<T>(string name);
        public void SetParameter<T>(string name, T value);
    }
}
