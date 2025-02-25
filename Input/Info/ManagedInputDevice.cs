using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shears.Input
{
    public struct ManagedInputDevice
    {
        public InputDevice Device { get; private set; }

        public ManagedInputDevice(InputDevice device)
        {
            Device = device;
        }

        #region Operators
        public static bool operator==(ManagedInputDevice left, ManagedInputDevice right)
        {
            return left.Device.deviceId == right.Device.deviceId;
        }

        public static bool operator!=(ManagedInputDevice left, ManagedInputDevice right)
        {
            return left.Device != right.Device;
        }

        public override readonly bool Equals(object obj)
        {
            return obj is ManagedInputDevice device &&
                   EqualityComparer<InputDevice>.Default.Equals(Device, device.Device);
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Device);
        }

        public readonly override string ToString() => Device.ToString();
        #endregion
    }
}
