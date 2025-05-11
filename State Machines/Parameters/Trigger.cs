using UnityEngine;

namespace Shears.StateMachines
{
    public struct Trigger
    {
        public static Trigger Inactive => new(false);
        public static Trigger Active => new(true);

        public bool Value { get; set; }

        public Trigger(bool value)
        {
            Value = value;
        }
    }
}
