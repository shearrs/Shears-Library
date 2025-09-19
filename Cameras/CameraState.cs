using Shears.Input;
using UnityEngine;

namespace Shears.Cameras
{
    public abstract class CameraState : MonoBehaviour
    {
        protected Transform CameraTransform { get; private set; }
        protected ManagedInputProvider InputProvider { get; private set; }

        public void SetGlobalValues(Transform cameraTransform, ManagedInputProvider inputProvider)
        {
            CameraTransform = cameraTransform;
            InputProvider = inputProvider;
        }

        public void Enter()
        {
            OnEnter();
        }

        public void UpdateState()
        {
            OnLateUpdate();
        }

        public void FixedUpdateState()
        {
            OnFixedUpdate();
        }

        public void Exit()
        {
            OnExit();
        }

        public virtual void Initialize() { }
        protected abstract void OnEnter();
        protected abstract void OnLateUpdate();
        protected virtual void OnFixedUpdate() { }
        protected abstract void OnExit();
    }
}
