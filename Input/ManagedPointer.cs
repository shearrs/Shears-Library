using UnityEngine;
using UnityEngine.InputSystem;

namespace Shears.Input
{
    public class ManagedPointer
    {
        /// <summary>
        /// Position within the coordinates of Unity's Display.
        /// </summary>
        public Vector2 Position { get; private set; }

        /// <summary>
        /// The current <see cref="ManagedPointer"/>.
        /// </summary>
        public static ManagedPointer Current => new(Pointer.current);

        private ManagedPointer(Pointer pointer)
        {
            Position = pointer.position.ReadValue();
        }

        public bool IsValid() => Pointer.current.deviceId != Pointer.InvalidDeviceId;
    }
}
