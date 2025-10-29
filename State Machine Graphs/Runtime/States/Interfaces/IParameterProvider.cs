using System;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public interface IParameterProvider
    {
        public SMID GetParameterID(string name);
        public T GetParameter<T>(string name);
        public T GetParameter<T>(SMID id);
        public void SetParameter<T>(string name, T value);
        public void SetParameter<T>(SMID id, T value);
    }
}
