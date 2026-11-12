using System;
using UnityEngine;

namespace Shears.HitDetection
{
    [Flags]
    public enum CastDirection
    {
        Up = 1 << 0,
        Down = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
        Forward = 1 << 4,
        Back = 1 << 5,
    }
}
