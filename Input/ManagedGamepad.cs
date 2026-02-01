using UnityEngine;
using UnityEngine.InputSystem;

namespace Shears.Input
{
    public class ManagedGamepad : ManagedInputType
    {
        /// <summary>
        /// The current <see cref="ManagedPointer"/>.
        /// </summary>
        public static ManagedGamepad Current => new(Gamepad.current);

        public bool WasUpdatedThisFrame => gamepad.wasUpdatedThisFrame;

        private readonly Gamepad gamepad;

        private ManagedGamepad(Gamepad gamepad)
        {
            this.gamepad = gamepad;
        }

        public bool IsValid() => Pointer.current.deviceId != Pointer.InvalidDeviceId;
    }
}
