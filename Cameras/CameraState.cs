using Shears.Logging;
using UnityEngine;

namespace Shears.Cameras
{
    public abstract class CameraState : SHMonoBehaviourLogger
    {
        protected Transform CameraTransform { get; private set; }
        protected CameraData GlobalData { get; private set; }

        public void SetGlobalValues(Transform cameraTransform, CameraData data)
        {
            CameraTransform = cameraTransform;
            GlobalData = data;
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
