using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [CreateAssetMenu(menuName = "Shears Library/State Machine Graph/Parameter List", order = 1)]
    public class ParameterList : ScriptableObject
    {
        [SerializeReference]
        private List<ParameterData> parameters = new();

        public IReadOnlyList<ParameterData> Parameters => parameters;
    }
}
