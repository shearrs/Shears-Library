using UnityEngine;
using UnityEngine.InputSystem;

namespace Shears.Input
{
    public class ManagedPointer : ManagedInputType
    {
        /// <summary>
        /// The current <see cref="ManagedPointer"/>.
        /// </summary>
        public static ManagedPointer Current => new(Pointer.current);

        /// <summary>
        /// Position within the coordinates of Unity's Display.
        /// </summary>
        public Vector2 Position => pointer.position.ReadValue();
        public Vector2 Delta => pointer.delta.ReadValue();

        public bool WasUpdatedThisFrame => pointer.wasUpdatedThisFrame;

        private readonly Pointer pointer;

        private ManagedPointer(Pointer pointer)
        {
            this.pointer = pointer;
        }

        public bool IsValid() => Pointer.current.deviceId != Pointer.InvalidDeviceId;
    }
}
