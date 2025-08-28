using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public interface IStateInjectable
    {
        public IReadOnlyCollection<Type> GetInjectableTypes();

        public bool CanInjectType(Type type);

        public void InjectType(object dependency);
    }
}
