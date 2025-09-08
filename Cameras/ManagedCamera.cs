using Shears.Input;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Cameras
{
    [RequireComponent(typeof(Camera))]
    public class ManagedCamera : MonoBehaviour
    {
        [SerializeField] private ManagedInputMap inputMap;
        [SerializeField] private List<CameraState> states = new();

        private CameraState currentState;

        public ManagedInputMap InputMap { get => inputMap; set => inputMap = value; }

        private void Awake()
        {
            foreach (var state in states)
            {
                state.SetGlobalValues(transform, inputMap);
                state.Initialize();
            }

            inputMap.EnableAllInputs();

            if (states.Count > 0)
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
    
        public void AddState(CameraState state)
        {
            states.Add(state);
        }

        public bool HasState(CameraState state)
        {
            return states.Contains(state);
        }
    }
}
