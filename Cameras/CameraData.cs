using System;
using UnityEngine;

namespace Shears.Cameras
{
    public class CameraData
    {
        public Func<Vector2> LookInput { get; set; }
        public Func<float> ZoomInput { get; set; }
        public Func<float> ScrollInput { get; set; }
        public Func<bool> RotateUp { get; set; }
        public Func<bool> RotateRight { get; set; }
        public Func<bool> RotateDown { get; set; }
        public Func<bool> RotateLeft { get; set; }
    }
}
