using System;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public interface IParameterProvider
    {
        public Guid GetParameterID(string name);
        public T GetParameter<T>(string name);
        public T GetParameter<T>(Guid id);
        public void SetParameter<T>(string name, T value);
        public void SetParameter<T>(Guid id, T value);
    }
}
