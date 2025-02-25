using UnityEngine;

namespace Shears.Input
{
    public class ManagedInputUser
    {
        public ManagedInputDevice Device { get; private set; }

        public ManagedInputUser(ManagedInputDevice device)
        {
            Device = device;
        }

        public override string ToString()
        {
            return "User: " + Device.Device;
        }
    }
}
