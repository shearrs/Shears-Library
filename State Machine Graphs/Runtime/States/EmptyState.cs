using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class EmptyState : State
    {
        protected override void OnEnter()
        {
        }

        protected override void OnExit()
        {
        }

        protected override void OnUpdate()
        {
        }
    }
}
