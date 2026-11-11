using System;
using UnityEngine;

namespace Shears.HitDetection
{
    [Flags]
    public enum CastDirection
    {
        Up = 0,
        Down = 1 << 0,
        Left = 1 << 1,
        Right = 1 << 2,
        Forward = 1 << 3,
        Back = 1 << 4,
    }
}
