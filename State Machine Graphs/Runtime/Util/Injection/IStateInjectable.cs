using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public interface IStateInjectable
    {
        public IReadOnlyCollection<Type> GetInjectableTypes();
    }
}
