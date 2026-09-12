using UnityEngine;

namespace Shears.Grids
{
    public class EntityPositionGroup
    {
        public EntityPosition Center { get; }
        public SurfaceEntityPosition Up { get; }
        public SurfaceEntityPosition Down { get; }
        public SurfaceEntityPosition Left { get; }
        public SurfaceEntityPosition Right { get; }

        public EntityPositionGroup(
            EntityPosition center,
            SurfaceEntityPosition up,
            SurfaceEntityPosition down,
            SurfaceEntityPosition left,
            SurfaceEntityPosition right
        )
        {
            Center = center;
            Up = up;
            Down = down;
            Left = left;
            Right = right;
        }

        public void Dispose()
        {
            Up?.Dispose();
            Down?.Dispose();
            Left?.Dispose();
            Right?.Dispose();
        }

        public bool TryGetPositionInDirection(
            Direction direction,
            out SurfaceEntityPosition position
        )
        {
            position = direction switch
            {
                Direction.Up => Up,
                Direction.Down => Down,
                Direction.Left => Left,
                Direction.Right => Right,
                _ => null,
            };

            return position != null;
        }

        public bool TryGetClosestPosition(Vector3 worldPosition, out EntityPosition closestPosition)
        {
            EntityPosition currentClosest = null;
            float minDistance = float.MaxValue;

            void testDistance(EntityPosition testTarget)
            {
                if (testTarget == null)
                    return;

                float distance = (testTarget.WorldPosition - worldPosition).sqrMagnitude;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    currentClosest = testTarget;
                }
            }

            testDistance(Up);
            testDistance(Down);
            testDistance(Left);
            testDistance(Right);

            if (currentClosest == null)
                testDistance(Center);

            closestPosition = currentClosest;
            return closestPosition != null;
        }
    }
}
