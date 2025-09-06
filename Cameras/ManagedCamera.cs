using Shears.Input;
using UnityEngine;

namespace ShearsLibrary.Cameras
{
    [RequireComponent(typeof(Camera))]
    public class ManagedCamera : MonoBehaviour
    {
        [SerializeField] private ManagedInputMap inputMap;
        [SerializeField] private CameraState[] states;

        private CameraState currentState;

        private void Awake()
        {
            foreach (var state in states)
            {
                state.SetGlobalValues(transform, inputMap);
                state.Initialize();
            }

            inputMap.EnableAllInputs();

            if (states.Length > 0)
                SetState(states[0]);
        }

        private void LateUpdate()
        {
            if (currentState != null)
                currentState.UpdateState();
        }

        private void SetState(CameraState state)
        {
            if (state == currentState)
                return;

            if (currentState != null)
                currentState.Exit();

            currentState = state;

            if (currentState != null)
                currentState.Enter();
        }
    }
}
