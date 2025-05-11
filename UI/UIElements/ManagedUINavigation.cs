using System.Collections.Generic;
using UnityEngine;

namespace Shears.UI
{
    [System.Serializable]
    public class ManagedUINavigation
    {
        public enum NavigationType { AutomaticStatic, AutomaticDynamic, Explicit }
        public enum Direction { Up, Right, Down, Left }

        [Tooltip("The type of navigation to use for this element.\n" +
                 "AutomaticStatic - Updates the navigation when the element is initialized.\n" +
                 "AutomaticDynamic - Updates the navigation every frame.\n" +
                 "Explicit - Uses the elements set in the inspector.")]
        [SerializeField] private NavigationType type;
        [SerializeField] private ManagedUIElement up;
        [SerializeField] private ManagedUIElement right;
        [SerializeField] private ManagedUIElement down;
        [SerializeField] private ManagedUIElement left;

        public NavigationType Type { get => type; set => type = value; }
        public ManagedUIElement Up { get => up; set => up = value; }
        public ManagedUIElement Right { get => right; set => right = value; }
        public ManagedUIElement Down { get => down; set => down = value; }
        public ManagedUIElement Left { get => left; set => left = value; }

        private ManagedUIElement element;

        public void Initialize(ManagedUIElement element)
        {
            this.element = element;

            ManagedUIEventSystem.OnNavigationChanged += StaticUpdate;
        }

        public void Uninitialize()
        {
            ManagedUIEventSystem.OnNavigationChanged -= StaticUpdate;
        }

        public void Update()
        {
            if (Type != NavigationType.AutomaticDynamic)
                return;

            UpdateNavigation();
        }

        public ManagedUIElement GetElement(Direction direction)
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

        public void SetElement(Direction direction, ManagedUIElement element)
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

        public void UpdateNavigation()
        {
            var elements = ManagedUIEventSystem.Elements;
            Vector3 position = element.transform.position;

            SetElement(Direction.Up, null);
            SetElement(Direction.Right, null);
            SetElement(Direction.Down, null);
            SetElement(Direction.Left, null);

            foreach (var element in elements)
            {
                if (element == this.element)
                    continue;

                var direction = GetDirectionToElement(element);
                var currentElement = GetElement(direction);

                if (currentElement == null) // if we don't already have an element in this direction
                    SetElement(direction, element);
                else if (Vector2.SqrMagnitude(position - element.transform.position) < Vector2.SqrMagnitude(position - currentElement.transform.position)) // if this element is closer
                    SetElement(direction, element);
            }
        }

        private Direction GetDirectionToElement(ManagedUIElement otherElement)
        {
            Vector2 direction = (otherElement.transform.position - element.transform.position).normalized;
            Vector2 right = Vector2.right;

            float angle = Vector2.SignedAngle(right, direction);

            if (angle <= 135 && angle > 45)
                return Direction.Up;
            else if (angle <= 45 && angle > -45)
                return Direction.Right;
            else if (angle <= -45 && angle > -135)
                return Direction.Down;
            else
                return Direction.Left;
        }

        private void StaticUpdate()
        {
            if (Type == NavigationType.AutomaticStatic)
                UpdateNavigation();
        }
    }
}
