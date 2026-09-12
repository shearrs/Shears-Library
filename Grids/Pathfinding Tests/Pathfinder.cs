using System.Collections.Generic;
using UnityEngine;
using static Shears.Grids.EntityPosition;
using static Shears.Grids.SurfaceNodeData;

namespace Shears.Grids
{
    public class Pathfinder : ShearsBehaviour
    {
        private const int CARDINAL_COST = 10;
        private const int DIAGONAL_COST = 14;
        private static readonly Vector3Int ZERO = Vector3Int.zero;
        private static readonly Vector3Int UP = Direction.Up.ToVectorInt();
        private static readonly Vector3Int DOWN = Direction.Down.ToVectorInt();
        private static readonly Vector3Int LEFT = Direction.Left.ToVectorInt();
        private static readonly Vector3Int RIGHT = Direction.Right.ToVectorInt();
        private static readonly Vector3Int FORWARD = Direction.Forward.ToVectorInt();
        private static readonly Vector3Int BACK = Direction.Back.ToVectorInt();
        private static readonly Vector3Int UP_RIGHT = UP + RIGHT;
        private static readonly Vector3Int UP_LEFT = UP + LEFT;
        private static readonly Vector3Int DOWN_RIGHT = DOWN + RIGHT;
        private static readonly Vector3Int DOWN_LEFT = DOWN + LEFT;

        [SerializeField]
        private bool drawGizmos = true;

        [SerializeField]
        private PathGrid grid;

        [SerializeField]
        private IPathEntity entity;

        [SerializeField]
        private IPathWeightBehaviour weightBehaviour;

        private readonly Heap<PathEntry> openSet = new(32);
        private Dictionary<EntityPosition, PathEntry> openSetMap;
        private HashSet<EntityPosition> closedSet;
        private List<EntityPosition> neighbors;
        private List<EntityPosition> entityPath;
        private List<PathPosition> path;
        private List<EntityPosition> registeredPositions;
        private Dictionary<EntityPosition, int> entityPathCountMap;
        private EntityPosition currentTarget;

        private Vector3 EntityPosition => Vector3.zero; // need to get enemy position

        private class PathEntry : IHeapItem<PathEntry>
        {
            public EntityPosition EntityPosition { get; }
            public PathEntry Parent { get; set; }
            public int HeapIndex { get; set; }
            public int GCost { get; set; } = 0;
            public int HCost { get; set; } = 0;
            public int FCost => GCost + HCost;

            public PathEntry(EntityPosition entityPosition)
            {
                EntityPosition = entityPosition;
            }

            public int CompareTo(PathEntry other)
            {
                int compare = FCost.CompareTo(other.FCost);

                if (compare == 0)
                    compare = HCost.CompareTo(other.HCost);

                return -compare;
            }
        }

        private void Awake()
        {
            CollectionUtil.GetPooled(out openSetMap);
            CollectionUtil.GetPooled(out closedSet);
            CollectionUtil.GetPooled(out neighbors);
            CollectionUtil.GetPooled(out entityPath);
            CollectionUtil.GetPooled(out path);
            CollectionUtil.GetPooled(out registeredPositions);
            CollectionUtil.GetPooled(out entityPathCountMap);
        }

        private void OnDestroy()
        {
            UnregisterPositions();

            CollectionUtil.ReleasePooled(openSetMap);
            CollectionUtil.ReleasePooled(closedSet);
            CollectionUtil.ReleasePooled(neighbors);
            CollectionUtil.ReleasePooled(entityPath);
            CollectionUtil.ReleasePooled(path);
            CollectionUtil.ReleasePooled(registeredPositions);
            CollectionUtil.ReleasePooled(entityPathCountMap);
        }

        public void RemoveTopPathPosition()
        {
            if (path.Count == 0)
                return;

            var position = path[0];
            var entityPosition = position.EntityPosition;

            path.RemoveAt(0);

            if (!entityPathCountMap.TryGetValue(entityPosition, out int pathCount))
                return;

            pathCount--;

            if (pathCount == 0)
                UnregisterPosition(entityPosition);
            else
                entityPathCountMap[entityPosition] = pathCount;
        }

        public void CalculatePath(
            Vector3 start,
            Vector3 target,
            Direction surfaceDirection = Direction.Down
        )
        {
            if (grid == null)
            {
                LogError("Grid is null!");
                Clear();
                return;
            }

            if (!grid.TryGetPositionGroup(start, out var positionGroup))
            {
                LogError($"Could not find starting position at: {start}.");
                Clear();
                return;
            }

            if (!grid.TryGetPositionGroup(target, out var targetPositionGroup))
            {
                LogError($"Could not find target position at: {target}.");
                Clear();
                return;
            }

            EntityPosition startPosition;

            if (!positionGroup.TryGetPositionInDirection(surfaceDirection, out var startSurface))
            {
                if (entity.CanWalkOnWalls)
                {
                    if (positionGroup.Down != null)
                        startPosition = positionGroup.Down;
                    else if (positionGroup.Left != null)
                        startPosition = positionGroup.Left;
                    else if (positionGroup.Right != null)
                        startPosition = positionGroup.Right;
                    else if (positionGroup.Up != null)
                        startPosition = positionGroup.Up;
                    else
                        startPosition = positionGroup.Center;
                }
                else if (positionGroup.Center != null)
                    startPosition = positionGroup.Center;
                else
                {
                    LogError($"Could not find valid starting position at {start}.");
                    Clear();
                    return;
                }
            }
            else
                startPosition = startSurface;

            if (
                !targetPositionGroup.TryGetPositionInDirection(
                    Direction.Down,
                    out var targetPosition
                )
            )
            {
                LogError($"Could not find floor position for target at: {target}.");
                Clear();
                return;
            }

            CalculatePath(startPosition, targetPosition);
        }

        public void CalculatePath(EntityPosition start, EntityPosition target)
        {
            if (grid == null)
            {
                LogError("Grid is null!");
                Clear();
                return;
            }
            else if (start == null || target == null)
            {
                LogError($"Invalid path request! Start = {start}, Target = {target}.");
                Clear();
                return;
            }

            currentTarget = target;

            Clear();
            UpdateEntityPath(start, target);
            UpdatePath(start);
            RegisterPositions();
        }

        private void UpdateEntityPath(EntityPosition start, EntityPosition target)
        {
            if (start == null || target == null)
            {
                LogWarning($"Invalid path request! Start: {start}, Target: {target}.");
                return;
            }
            else if (start == target)
            {
                LogWarning("Start is the same as target.");
                currentTarget = target;
                return;
            }

            PathEntry fallbackTarget = null;

            var startEntry = new PathEntry(start);
            openSet.Enqueue(startEntry);
            openSetMap[start] = startEntry;

            while (openSet.Count > 0)
            {
                var currentEntry = openSet.Dequeue();
                var currentPosition = currentEntry.EntityPosition;
                closedSet.Add(currentPosition);

                if (currentEntry.EntityPosition == target)
                {
                    RetracePath(startEntry, currentEntry);
                    return;
                }

                GetNeighbors(currentPosition, neighbors);

                if (
                    currentPosition.TryGetData(out DoorwayNodeData doorData) && doorData.IsConnected
                )
                {
                    if (grid.TryGetPositionGroup(doorData.ConnectedGridPosition, out var group))
                    {
                        if (group.Down is SurfaceEntityPosition exitPosition)
                            neighbors.Add(exitPosition);
                    }
                }

                foreach (var neighbor in neighbors)
                {
                    if (neighbor == null || closedSet.Contains(neighbor))
                        continue;

                    int totalMovementCost =
                        currentEntry.GCost
                        + GetDistance(currentPosition, neighbor)
                        + GetWeight(currentPosition, neighbor);

                    bool inOpenSet = openSetMap.TryGetValue(neighbor, out var neighborEntry);

                    if (!inOpenSet || neighborEntry.GCost > totalMovementCost)
                    {
                        neighborEntry ??= new(neighbor);

                        neighborEntry.GCost = totalMovementCost;
                        neighborEntry.HCost = GetDistance(neighbor, target);
                        neighborEntry.Parent = currentEntry;

                        if (fallbackTarget == null || neighborEntry.HCost < fallbackTarget.HCost)
                            fallbackTarget = neighborEntry;

                        if (!inOpenSet)
                        {
                            openSet.Enqueue(neighborEntry);
                            openSetMap[neighbor] = neighborEntry;
                        }
                    }
                }
            }

            if (fallbackTarget != null)
                RetracePath(startEntry, fallbackTarget);
            else
                entityPath.Clear();
        }

        private void UpdatePath(EntityPosition start)
        {
            if (entityPath.Count == 0)
                return;

            var initialTarget = entityPath[0];

            if (
                initialTarget is SurfaceEntityPosition initialSurface
                && start is SurfaceEntityPosition startSurface
            )
            {
                if (startSurface.IsSlope)
                {
                    var slopeOffset = GetSlopeOffset(startSurface, 0.01f);
                    var slopePosition = startSurface.WorldPosition;
                    var connectingPosition = slopePosition + slopeOffset;
                    var targetSurface = initialSurface.WorldPosition;
                    var currentSqrDistance = (targetSurface - EntityPosition).sqrMagnitude;
                    var sqrSlopeDistance = (targetSurface - slopePosition).sqrMagnitude;
                    var sqrConnectingDistance = (targetSurface - connectingPosition).sqrMagnitude;

                    if (sqrSlopeDistance < currentSqrDistance)
                    {
                        AddPathPosition(new(start, slopePosition, startSurface.SurfaceNormal));
                        AddPathPosition(new(start, connectingPosition, startSurface.SurfaceNormal));
                    }
                    else if (sqrConnectingDistance < currentSqrDistance)
                        AddPathPosition(new(start, connectingPosition, startSurface.SurfaceNormal));
                }

                if (startSurface.SurfaceDirection != initialSurface.SurfaceDirection)
                {
                    if (startSurface.SurfaceGridPosition == initialSurface.SurfaceGridPosition)
                    {
                        var bridgingOffset = GetBridgingOffset(startSurface);
                        AddPathPosition(
                            new(
                                initialTarget,
                                initialSurface.WorldPosition + bridgingOffset,
                                initialSurface.SurfaceNormal
                            )
                        );
                    }
                }
            }

            for (int i = 0; i < entityPath.Count; i++)
            {
                var current = entityPath[i];
                var previous = i == 0 ? start : entityPath[i - 1];

                CreatePathPositions(previous, current);
            }
        }

        private void RetracePath(PathEntry startEntry, PathEntry targetEntry)
        {
            var current = targetEntry;

            while (current != startEntry && current != null)
            {
                entityPath.Add(current.EntityPosition);
                current = current.Parent;
            }

            if (entityPath.Count == 0)
            {
                entityPath.Add(startEntry.EntityPosition);
                return;
            }

            entityPath.Reverse();
        }

        private void GetNeighbors(EntityPosition currentPosition, List<EntityPosition> neighbors)
        {
            neighbors.Clear();

            if (entity.CanWalkOnWalls)
                GetNeighborsCanWalkOnWalls(currentPosition, neighbors);
            else
                GetNeighborsDefault(currentPosition, neighbors);
        }

        private void GetNeighborsDefault(
            EntityPosition currentPosition,
            List<EntityPosition> neighbors
        )
        {
            var gridPosition = currentPosition.GridPosition;

            if (!grid.TryGetPositionGroup(gridPosition, out var group))
                return;

            void addNeighbor(EntityPosition neighbor)
            {
                if (neighbor != null)
                    neighbors.Add(neighbor);
            }

            bool isValidSlope(EntityPosition position, SlopeDirection direction)
            {
                return position is SurfaceEntityPosition surface
                    && surface.IsSlope
                    && surface.SlopeDirection == direction;
            }

            bool tryGetGroup(Direction direction, out EntityPositionGroup targetGroup)
            {
                targetGroup = default;

                if (
                    group.TryGetPositionInDirection(direction, out var localPosition)
                    && localPosition is SurfaceEntityPosition
                )
                    return false;

                return grid.TryGetPositionGroup(
                    gridPosition + direction.ToVectorInt(),
                    out targetGroup
                );
            }

            bool tryGetGroupFromOffset(Vector3Int offset, out EntityPositionGroup targetGroup)
            {
                return grid.TryGetPositionGroup(gridPosition + offset, out targetGroup);
            }

            if (currentPosition is SurfaceEntityPosition currentSurface)
            {
                if (currentSurface.IsSlope)
                {
                    if (currentSurface.SlopeDirection == SlopeDirection.UpRight)
                    {
                        if (tryGetGroup(Direction.Left, out var leftGroup))
                        {
                            addNeighbor(leftGroup.Down);

                            if (isValidSlope(leftGroup.Center, SlopeDirection.UpLeft))
                                addNeighbor(leftGroup.Center);
                        }

                        if (tryGetGroupFromOffset(UP_RIGHT, out var upRightGroup))
                        {
                            addNeighbor(upRightGroup.Down);

                            if (isValidSlope(upRightGroup.Center, SlopeDirection.UpRight))
                                addNeighbor(upRightGroup.Center);
                        }
                    }
                    else if (currentSurface.SlopeDirection == SlopeDirection.UpLeft)
                    {
                        if (tryGetGroup(Direction.Right, out var rightGroup))
                        {
                            addNeighbor(rightGroup.Down);

                            if (isValidSlope(rightGroup.Center, SlopeDirection.UpRight))
                                addNeighbor(rightGroup.Center);
                        }

                        if (tryGetGroupFromOffset(UP_LEFT, out var upLeftGroup))
                        {
                            addNeighbor(upLeftGroup.Down);

                            if (isValidSlope(upLeftGroup.Center, SlopeDirection.UpLeft))
                                addNeighbor(upLeftGroup.Center);
                        }
                    }
                }
                else
                {
                    if (tryGetGroup(Direction.Left, out var leftGroup))
                    {
                        addNeighbor(leftGroup.Down);

                        if (isValidSlope(leftGroup.Center, SlopeDirection.UpLeft))
                            addNeighbor(leftGroup.Center);
                    }

                    if (tryGetGroup(Direction.Right, out var rightGroup))
                    {
                        addNeighbor(rightGroup.Down);

                        if (isValidSlope(rightGroup.Center, SlopeDirection.UpRight))
                            addNeighbor(rightGroup.Center);
                    }

                    if (tryGetGroup(Direction.Forward, out var forwardGroup))
                        addNeighbor(forwardGroup.Down);

                    if (tryGetGroup(Direction.Back, out var backGroup))
                        addNeighbor(backGroup.Down);
                }
            }
            else
            {
                if (group.Down != null)
                    addNeighbor(group.Down);
                else if (tryGetGroup(Direction.Down, out var downGroup))
                {
                    addNeighbor(downGroup.Down);
                    addNeighbor(downGroup.Center);
                }
            }
        }

        private void GetNeighborsCanWalkOnWalls(
            EntityPosition currentPosition,
            List<EntityPosition> neighbors
        )
        {
            var gridPosition = currentPosition.GridPosition;

            if (!grid.TryGetPositionGroup(gridPosition, out var group))
                return;

            void addNeighbor(EntityPosition neighbor)
            {
                if (neighbor != null)
                    neighbors.Add(neighbor);
            }

            bool isValidSlope(EntityPosition position, SlopeDirection direction)
            {
                return position is SurfaceEntityPosition surface
                    && surface.IsSlope
                    && surface.SlopeDirection == direction;
            }

            bool tryGetGroup(Direction direction, out EntityPositionGroup targetGroup)
            {
                targetGroup = default;

                if (
                    group.TryGetPositionInDirection(direction, out var localPosition)
                    && localPosition is not null
                )
                    return false;

                return grid.TryGetPositionGroup(
                    gridPosition + direction.ToVectorInt(),
                    out targetGroup
                );
            }

            bool tryGetGroupFromOffset(Vector3Int offset, out EntityPositionGroup targetGroup)
            {
                return grid.TryGetPositionGroup(gridPosition + offset, out targetGroup);
            }

            if (currentPosition is SurfaceEntityPosition currentSurface)
            {
                if (currentSurface.IsSlope)
                {
                    if (currentSurface.SlopeDirection == SlopeDirection.UpRight)
                    {
                        if (tryGetGroup(Direction.Left, out var leftGroup))
                        {
                            addNeighbor(leftGroup.Down);

                            if (isValidSlope(leftGroup.Center, SlopeDirection.UpLeft))
                                addNeighbor(leftGroup.Center);
                        }

                        if (tryGetGroupFromOffset(UP_RIGHT, out var upRightGroup))
                        {
                            addNeighbor(upRightGroup.Down);

                            if (isValidSlope(upRightGroup.Center, SlopeDirection.UpRight))
                                addNeighbor(upRightGroup.Center);
                        }

                        if (tryGetGroup(Direction.Up, out var upGroup))
                            addNeighbor(upGroup.Right);
                    }
                    else if (currentSurface.SlopeDirection == SlopeDirection.UpLeft)
                    {
                        if (tryGetGroup(Direction.Right, out var rightGroup))
                        {
                            addNeighbor(rightGroup.Down);

                            if (isValidSlope(rightGroup.Center, SlopeDirection.UpRight))
                                addNeighbor(rightGroup.Center);
                        }

                        if (tryGetGroupFromOffset(UP_LEFT, out var upLeftGroup))
                        {
                            addNeighbor(upLeftGroup.Down);

                            if (isValidSlope(upLeftGroup.Center, SlopeDirection.UpLeft))
                                addNeighbor(upLeftGroup.Center);
                        }

                        if (tryGetGroup(Direction.Up, out var upGroup))
                            addNeighbor(upGroup.Left);
                    }
                    else if (currentSurface.SlopeDirection == SlopeDirection.DownRight)
                    {
                        if (tryGetGroup(Direction.Left, out var leftGroup))
                        {
                            addNeighbor(leftGroup.Up);

                            if (isValidSlope(leftGroup.Center, SlopeDirection.DownLeft))
                                addNeighbor(leftGroup.Center);
                        }

                        if (tryGetGroupFromOffset(DOWN_RIGHT, out var downRightGroup))
                        {
                            addNeighbor(downRightGroup.Up);

                            if (isValidSlope(downRightGroup.Center, SlopeDirection.DownRight))
                                addNeighbor(downRightGroup.Center);
                        }

                        if (tryGetGroup(Direction.Down, out var downGroup))
                            addNeighbor(downGroup.Right);
                    }
                    else if (currentSurface.SlopeDirection == SlopeDirection.DownLeft)
                    {
                        if (tryGetGroup(Direction.Right, out var rightGroup))
                        {
                            addNeighbor(rightGroup.Up);

                            if (isValidSlope(rightGroup.Center, SlopeDirection.DownRight))
                                addNeighbor(rightGroup.Center);
                        }

                        if (tryGetGroupFromOffset(DOWN_LEFT, out var downLeftGroup))
                        {
                            addNeighbor(downLeftGroup.Up);

                            if (isValidSlope(downLeftGroup.Center, SlopeDirection.DownLeft))
                                addNeighbor(downLeftGroup.Center);
                        }
                    }
                }
                else
                {
                    void addLocalLeftAndRightSurfaces(
                        Direction left,
                        Direction right,
                        SlopeDirection localUpLeft,
                        SlopeDirection localUpRight,
                        SlopeDirection localDownLeft,
                        SlopeDirection localDownRight,
                        Vector3Int downLeftOffset,
                        Vector3Int downRightOffset
                    )
                    {
                        if (group.TryGetPositionInDirection(left, out var leftPosition))
                            addNeighbor(leftPosition);
                        else if (tryGetGroup(left, out var leftGroup))
                        {
                            if (isValidSlope(leftGroup.Center, localUpLeft))
                                addNeighbor(leftGroup.Center);

                            if (
                                leftGroup.TryGetPositionInDirection(
                                    currentSurface.SurfaceDirection,
                                    out var leftSurface
                                )
                            )
                                addNeighbor(leftSurface);
                        }

                        if (group.TryGetPositionInDirection(right, out var rightPosition))
                            addNeighbor(rightPosition);
                        else if (tryGetGroup(right, out var rightGroup))
                        {
                            if (isValidSlope(rightGroup.Center, localUpRight))
                                addNeighbor(rightGroup.Center);

                            if (
                                rightGroup.TryGetPositionInDirection(
                                    currentSurface.SurfaceDirection,
                                    out var rightSurface
                                )
                            )
                                addNeighbor(rightSurface);
                        }

                        if (tryGetGroupFromOffset(downLeftOffset, out var downLeftGroup))
                        {
                            if (isValidSlope(downLeftGroup.Center, localDownLeft))
                                addNeighbor(downLeftGroup.Center);
                            else if (
                                downLeftGroup.TryGetPositionInDirection(
                                    right,
                                    out var downLeftRightPosition
                                )
                            )
                                addNeighbor(downLeftRightPosition);
                        }

                        if (tryGetGroupFromOffset(downRightOffset, out var downRightGroup))
                        {
                            if (isValidSlope(downRightGroup.Center, localDownRight))
                                addNeighbor(downRightGroup.Center);
                            else if (
                                downRightGroup.TryGetPositionInDirection(
                                    left,
                                    out var downRightLeftPosition
                                )
                            )
                                addNeighbor(downRightLeftPosition);
                        }
                    }

                    switch (currentSurface.SurfaceDirection)
                    {
                        case Direction.Up:
                            addLocalLeftAndRightSurfaces(
                                Direction.Right,
                                Direction.Left,
                                SlopeDirection.DownRight,
                                SlopeDirection.DownLeft,
                                SlopeDirection.UpRight,
                                SlopeDirection.UpLeft,
                                UP_RIGHT,
                                UP_LEFT
                            );
                            break;
                        case Direction.Down:
                            addLocalLeftAndRightSurfaces(
                                Direction.Left,
                                Direction.Right,
                                SlopeDirection.UpLeft,
                                SlopeDirection.UpRight,
                                SlopeDirection.DownLeft,
                                SlopeDirection.DownRight,
                                DOWN_LEFT,
                                DOWN_RIGHT
                            );
                            break;
                        case Direction.Left:
                            addLocalLeftAndRightSurfaces(
                                Direction.Up,
                                Direction.Down,
                                SlopeDirection.DownLeft,
                                SlopeDirection.UpLeft,
                                SlopeDirection.DownRight,
                                SlopeDirection.UpRight,
                                UP_LEFT,
                                DOWN_LEFT
                            );
                            break;
                        case Direction.Right:
                            addLocalLeftAndRightSurfaces(
                                Direction.Down,
                                Direction.Up,
                                SlopeDirection.UpRight,
                                SlopeDirection.DownRight,
                                SlopeDirection.UpLeft,
                                SlopeDirection.DownLeft,
                                DOWN_RIGHT,
                                UP_RIGHT
                            );
                            break;
                    }

                    if (
                        tryGetGroup(Direction.Forward, out var forwardGroup)
                        && forwardGroup.TryGetPositionInDirection(
                            currentSurface.SurfaceDirection,
                            out var forwardSurface
                        )
                    )
                        addNeighbor(forwardSurface);

                    if (
                        tryGetGroup(Direction.Back, out var backGroup)
                        && backGroup.TryGetPositionInDirection(
                            currentSurface.SurfaceDirection,
                            out var backSurface
                        )
                    )
                        addNeighbor(backSurface);
                }
            }
            else
            {
                if (group.Down != null)
                    addNeighbor(group.Down);
                else if (tryGetGroup(Direction.Down, out var downGroup))
                {
                    addNeighbor(downGroup.Down);
                    addNeighbor(downGroup.Center);
                }
            }
        }

        private int GetDistance(EntityPosition a, EntityPosition b)
        {
            if (
                a.TryGetData(out DoorwayNodeData doorData)
                && doorData.IsConnected
                && doorData.ConnectedGridPosition == b.GridPosition
            )
                return CARDINAL_COST;

            var aPosition = a.GridPosition;
            var bPosition = b.GridPosition;

            int xDistance = Mathf.Abs(aPosition.x - bPosition.x);
            int yDistance = Mathf.Abs(aPosition.y - bPosition.y);
            int zDistance = Mathf.Abs(aPosition.z - bPosition.z);

            if (xDistance == 0 && yDistance > 1 && zDistance == 0) // this is a fall
                return CARDINAL_COST;

            int min = Mathf.Min(xDistance, yDistance, zDistance);
            int distance =
                DIAGONAL_COST * min
                + CARDINAL_COST * (xDistance + yDistance + zDistance - (2 * min));

            if (a is SurfaceEntityPosition surfaceA && b is SurfaceEntityPosition surfaceB)
                distance += GetSurfaceDistance(surfaceA, surfaceB);

            return distance;
        }

        private int GetSurfaceDistance(SurfaceEntityPosition a, SurfaceEntityPosition b)
        {
            if (a.SurfaceDirection == b.SurfaceDirection)
                return 0;
            else
                return CARDINAL_COST;
        }

        private int GetWeight(EntityPosition currentPosition, EntityPosition targetPosition)
        {
            weightBehaviour ??= new DefaultWeightBehaviour();

            return weightBehaviour.GetWeight(new(currentPosition, targetPosition, entity));
        }

        private Vector3 GetBridgingOffset(SurfaceEntityPosition previousPosition)
        {
            return 0.45f * previousPosition.SurfaceNormal;
        }

        private Vector3 GetSlopeOffset(SurfaceEntityPosition surface, float extraOffset = 0.0f)
        {
            var nodeSize = (0.5f + extraOffset) * grid.NodeSize;

            return surface.SlopeDirection switch
            {
                SurfaceNodeData.SlopeDirection.UpLeft => new Vector3(-nodeSize, nodeSize, 0),
                SurfaceNodeData.SlopeDirection.UpRight => new Vector3(nodeSize, nodeSize, 0),
                SurfaceNodeData.SlopeDirection.DownLeft => new Vector3(-nodeSize, -nodeSize, 0),
                SurfaceNodeData.SlopeDirection.DownRight => new Vector3(nodeSize, -nodeSize, 0),
                _ => Vector3.zero,
            };
        }

        private void CreatePathPositions(
            EntityPosition previousPosition,
            EntityPosition currentPosition
        )
        {
            if (currentPosition is not SurfaceEntityPosition currentSurface)
            {
                AddPathPosition(
                    new(
                        currentPosition,
                        currentPosition.WorldPosition,
                        grid.transform.TransformDirection(Vector3.up)
                    )
                );
                return;
            }

            if (previousPosition is not SurfaceEntityPosition previousSurface)
            {
                AddPathPosition(
                    new(currentPosition, currentSurface.WorldPosition, currentSurface.SurfaceNormal)
                );
                return;
            }

            if (!currentSurface.IsSlope)
            {
                if (previousSurface.IsSlope)
                {
                    var slopeOffset = GetSlopeOffset(previousSurface, 0.01f);
                    var slopePosition = previousSurface.WorldPosition + slopeOffset;

                    AddPathPosition(
                        new(previousPosition, slopePosition, previousSurface.SurfaceNormal)
                    );
                }
                else if (previousSurface.SurfaceDirection != currentSurface.SurfaceDirection)
                {
                    if (previousSurface.SurfaceGridPosition == currentSurface.SurfaceGridPosition)
                    {
                        var bridgingOffset = GetBridgingOffset(previousSurface);
                        AddPathPosition(
                            new(
                                currentPosition,
                                currentSurface.WorldPosition + bridgingOffset,
                                currentSurface.SurfaceNormal
                            )
                        );
                    }
                }

                AddPathPosition(
                    new(currentPosition, currentSurface.WorldPosition, currentSurface.SurfaceNormal)
                );
                return;
            }

            var offset = GetSlopeOffset(currentSurface);

            if (
                !previousSurface.IsSlope
                || previousSurface.SlopeDirection != currentSurface.SlopeDirection
            )
                AddPathPosition(
                    new(
                        currentPosition,
                        currentSurface.WorldPosition - offset,
                        currentSurface.SurfaceNormal
                    )
                );

            AddPathPosition(
                new(currentPosition, currentSurface.WorldPosition, currentSurface.SurfaceNormal)
            );
        }

        private void AddPathPosition(PathPosition pathPosition)
        {
            var entityPosition = pathPosition.EntityPosition;

            path.Add(pathPosition);

            if (entityPathCountMap.TryGetValue(entityPosition, out int count))
                entityPathCountMap[entityPosition] = count + 1;
            else
                entityPathCountMap[entityPosition] = 1;
        }

        private void Clear()
        {
            UnregisterPositions();

            entityPath.Clear();
            registeredPositions.Clear();
            entityPathCountMap.Clear();
            path.Clear();
            openSet.Clear();
            openSetMap.Clear();
            closedSet.Clear();
        }

        private void RegisterPositions()
        {
            foreach (var position in entityPath)
            {
                position.RegisterEntity(entity);
                position.Updated += OnPositionUpdated;
                registeredPositions.Add(position);
            }
        }

        private void UnregisterPositions()
        {
            if (registeredPositions == null)
                return;

            for (int i = 0; i < registeredPositions.Count; i++)
                UnregisterPosition(registeredPositions[0]);
        }

        private void UnregisterPosition(EntityPosition position)
        {
            if (position == null)
                return;

            position.UnregisterEntity(entity);
            registeredPositions.Remove(position);
            entityPathCountMap.Remove(position);
        }

        private void OnPositionUpdated(EntityPositionUpdateData data)
        {
            if (currentTarget == null)
            {
                UnregisterPositions();
                return;
            }

            if (data.Data is not PathNodeData pathData)
                return;

            if (pathData.IsBlocked)
            {
                int target;

                for (target = 0; target < entityPath.Count; target++)
                {
                    var position = entityPath[target];

                    if (position == data.Position)
                        break;
                }

                while (entityPath.Count > target)
                {
                    var position = entityPath[^1];

                    position.UnregisterEntity(entity);
                    position.Updated -= OnPositionUpdated;

                    entityPath.RemoveAt(entityPath.Count - 1);
                }
            }
            else
            {
                var position = entity.EntityPosition;

                if (position == null)
                {
                    if (!grid.TryGetPositionGroup(entity.Position, out var positionGroup))
                    {
                        LogError($"Could not find starting position at: {entity.Position}.");
                        return;
                    }
                    else if (positionGroup.Down != null)
                        position = positionGroup.Down;
                    else if (positionGroup.Center != null)
                        position = positionGroup.Center;
                    else
                    {
                        LogError(
                            $"Could not find valid position direction in group at: {entity.Position}."
                        );
                        return;
                    }
                }

                CalculatePath(position, currentTarget);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos || grid == null || entityPath == null || path == null)
                return;

            Gizmos.color = Color.magenta;

            if (grid.TryGetPositionGroup(entity.Position, out var positionGroup))
            {
                var currentEntityPosition = positionGroup.Center;

                if (!entity.CanWalkOnWalls && positionGroup.Down != null)
                    currentEntityPosition = positionGroup.Down;
                else if (entity.CanWalkOnWalls)
                    positionGroup.TryGetClosestPosition(entity.Position, out currentEntityPosition);

                if (currentEntityPosition != null)
                {
                    GizmosUtil.DrawWireDisc(currentEntityPosition.WorldPosition, Vector3.up, 0.25f);

                    if (path.Count > 0)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(currentEntityPosition.WorldPosition, path[0].WorldPosition);
                    }
                }
            }

            if (entityPath.Count == 0 || path.Count == 0)
                return;

            Gizmos.color = Color.green;
            GizmosUtil.DrawWireDisc(entityPath[^1].WorldPosition, Vector3.up, 0.25f);

            for (int i = 0; i < path.Count; i++)
            {
                if (i == path.Count - 1)
                    break;

                var current = path[i];
                var nextPosition = path[i + 1];

                if (
                    current.TryGetData(out DoorwayNodeData doorData)
                    && nextPosition.GridPosition == doorData.ConnectedGridPosition
                )
                    Gizmos.color = Color.yellow.With(a: 0.45f);
                else
                    Gizmos.color = Color.red;

                Gizmos.DrawLine(nextPosition.WorldPosition, current.WorldPosition);

                if (openSetMap.TryGetValue(current.EntityPosition, out var entry))
                {
                    int weight =
                        entry.FCost
                        + GetWeight(current.EntityPosition, nextPosition.EntityPosition);
                    GizmosUtil.DrawText(current.WorldPosition, weight);
                }
            }
        }
    }
}
