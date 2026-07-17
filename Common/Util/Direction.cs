using UnityEngine;

namespace Shears
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        Forward,
        Back,
    }

    public static class DirectionUtil
    {
        public static Vector3 ToVector(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Vector3.up,
                Direction.Down => Vector3.down,
                Direction.Left => Vector3.left,
                Direction.Right => Vector3.right,
                Direction.Forward => Vector3.forward,
                Direction.Back => Vector3.back,
                _ => Vector3.zero,
            };
        }

        public static Vector3Int ToVectorInt(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Vector3Int.up,
                Direction.Down => Vector3Int.down,
                Direction.Left => Vector3Int.left,
                Direction.Right => Vector3Int.right,
                Direction.Forward => Vector3Int.forward,
                Direction.Back => Vector3Int.back,
                _ => Vector3Int.zero,
            };
        }
    }
}
