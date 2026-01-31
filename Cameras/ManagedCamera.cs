using Shears.Input;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Cameras
{
    [RequireComponent(typeof(Camera))]
    public class ManagedCamera : MonoBehaviour
    {
        [SerializeField] private bool initializeOnAwake = true;
        [SerializeField] private ManagedInputProvider inputMap;
        [SerializeField] private List<CameraState> states = new();

        private Camera rawCamera;
        private CameraState currentState;

        public bool InitializeOnAwake { get => initializeOnAwake; set => initializeOnAwake = value; }
        public ManagedInputProvider Input { get => inputMap; set => inputMap = value; }
        public Camera RawCamera => rawCamera;

        private void Awake()
        {
            if (initializeOnAwake)
                Initialize();
        }

        private void OnDisable()
        {
            if (currentState != null)
                currentState.Exit();
        }

        private void LateUpdate()
        {
            if (currentState != null)
                currentState.UpdateState();
        }

        private void FixedUpdate()
        {
            if (currentState != null)
                currentState.FixedUpdateState();
        }

        public void Initialize()
        {
            rawCamera = GetComponent<Camera>();

            foreach (var state in states)
            {
                state.SetGlobalValues(transform, inputMap);
                state.Initialize();
            }

            if (states.Count > 0)
                SetState(states[0]);
        }

        public void SetState(CameraState state)
        {
            if (state == currentState)
                return;

            if (currentState != null)
                currentState.Exit();

            currentState = state;

            if (currentState != null)
                currentState.Enter();
        }

        public void SetState<T>() where T : CameraState
        {
            var type = typeof(T);

            if (currentState != null && type == currentState.GetType())
                return;

            foreach (var state in states)
            {
                if (state.GetType() == type)
                {
                    SetState(state);
                    break;
                }    
            }
        }
    
        public void AddState(CameraState state)
        {
            state.SetGlobalValues(transform, inputMap);
            state.Initialize();
            states.Add(state);
        }

        public bool HasState(CameraState state)
        {
            return states.Contains(state);
        }
    }
}
