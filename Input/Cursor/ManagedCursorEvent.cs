using UnityEngine;

namespace Shears.Input
{
    public class ManagedCursorEvent : MonoBehaviour
    {
        [SerializeField] private bool invokeOnAwake;
        [SerializeField] private bool cursorVisible;
        [SerializeField] private CursorLockMode lockMode;

        private void Awake()
        {
            if (invokeOnAwake)
                Invoke();
        }

        public void Invoke()
        {
            CursorManager.SetCursorVisibility(cursorVisible);
            CursorManager.SetCursorLockMode(lockMode);
        }
    }
}
