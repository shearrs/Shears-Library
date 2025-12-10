using System;
using UnityEngine;

namespace Shears.Input
{
    public abstract class ManagedInputProvider : ScriptableObject
    {
        public abstract IManagedInput GetInput(string name);

        public abstract void GetInputs(params (string name, Action<IManagedInput> action)[] inputs);

        public abstract ManagedInputGroup GetInputGroup(params (string name, ManagedInputPhase phase, ManagedInputEventWithInfo action)[] bindings);
    }
}
