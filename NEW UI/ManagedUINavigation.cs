using UnityEngine;

namespace Shears.UI
{
    [System.Serializable]
    public struct ManagedUINavigation
    {
        public enum Direction { Up, Right, Down, Left }

        [field: SerializeField] public ManagedUIElement Up { get; set; }
        [field: SerializeField] public ManagedUIElement Right { get; set; }
        [field: SerializeField] public ManagedUIElement Down { get; set; }
        [field: SerializeField] public ManagedUIElement Left { get; set; }

        public void SetElement(ManagedUIElement element, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    Up = element;
                    break;
                case Direction.Right: 
                    Right = element; 
                    break;
                case Direction.Down: 
                    Down = element;
                    break;
                case Direction.Left:
                    Left = element;
                    break;
            }
        }

        public readonly ManagedUIElement GetElement(Direction direction)
        {
            return direction switch
            {
                Direction.Up => Up,
                Direction.Right => Right,
                Direction.Down => Down,
                Direction.Left => Left,
                _ => null,
            };
        }
    }
}
