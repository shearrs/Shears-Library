using System.Collections.Generic;
using UnityEngine;

namespace Shears.Grids
{
    public class Pathfinder : ShearsBehaviour
    {
        private const int CARDINAL_COST = 10;
        private const int DIAGONAL_COST = 14;

        [SerializeField]
        private PathGrid grid;

        private IPathEntity entity; // CHANGE TO ENEMY

        private readonly Heap<PathEntry> openSet = new(32);
        private Dictionary<EntityPosition, PathEntry> openSetMap;
        private HashSet<EntityPosition> closedSet;
        private List<EntityPosition> neighbors;
        private List<EntityPosition> entityPath;
        private List<PathPosition> path;
        private List<EntityPosition> registeredPositions;
        private EntityPosition currentTarget;

        private bool CanWalkOnWalls => false; // need to get this from the enemy
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
        }

        private void OnDestroy()
        {
            CollectionUtil.ReleasePooled(openSetMap);
            CollectionUtil.ReleasePooled(closedSet);
            CollectionUtil.ReleasePooled(neighbors);
            CollectionUtil.ReleasePooled(entityPath);
            CollectionUtil.ReleasePooled(path);
            CollectionUtil.ReleasePooled(registeredPositions);
        }

        public void CalculatePath(EntityPosition start, EntityPosition target)
        {
            if (grid == null)
            {
                LogError("Grid is null!");
                return;
            }
            else if (start == null || target == null)
            {
                LogError($"Invalid path request! Start = {start}, Target = {target}.");
                return;
            }
            else if (start == target)
            {
                path.Clear();

                return;
            }

            foreach (var position in registeredPositions)
                position?.UnregisterEntity(entity);

            registeredPositions.Clear();

            UpdatePath(start, target);

            if (entityPath.Count > 0)
            {
                foreach (var position in entityPath)
                {
                    position.RegisterEntity(entity);
                    registeredPositions.Add(position);
                }

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
                        var sqrConnectingDistance = (
                            targetSurface - connectingPosition
                        ).sqrMagnitude;

                        if (sqrSlopeDistance < currentSqrDistance)
                        {
                            path.Add(new(slopePosition, startSurface.SurfaceNormal));
                            path.Add(new(connectingPosition, startSurface.SurfaceNormal));
                        }
                        else if (sqrConnectingDistance < currentSqrDistance)
                            path.Add(new(connectingPosition, startSurface.SurfaceNormal));
                    }
                }
            }
        }

        private void UpdatePath(EntityPosition start, EntityPosition target)
        {
            PathEntry fallbackTarget = null;

            entityPath.Clear();
            path.Clear();
            openSet.Clear();
            openSetMap.Clear();
            closedSet.Clear();

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

                grid.GetNeighbors(currentPosition, neighbors);

                if (
                    neighbors[1] == null
                    && grid.TryGetClosestFloorPosition(currentPosition, out var downNeighbor)
                )
                    neighbors[1] = downNeighbor;

                if (
                    currentPosition.TryGetData(out DoorwayNodeData doorData) && doorData.IsConnected
                )
                {
                    if (
                        grid.TryGetGroundPosition(
                            doorData.ConnectedGridPosition,
                            out var doorPosition
                        )
                    )
                        neighbors.Add(doorPosition);
                }

                foreach (var neighbor in neighbors)
                {
                    if (closedSet.Contains(neighbor) || !IsValidMove(currentPosition, neighbor))
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

                        if (inOpenSet)
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

        private bool IsValidMove(EntityPosition currentPosition, EntityPosition targetPosition)
        {
            if (targetPosition is not SurfaceEntityPosition targetSurface)
                return IsValidFall(currentPosition, targetPosition);

            bool validTarget = IsValidGround(targetSurface);

            if (!validTarget)
                return false;

            if (currentPosition is SurfaceEntityPosition currentSurface)
                return IsValidDirection(currentSurface, targetSurface);
            else
                return IsValidFall(currentPosition, targetPosition);
        }

        private bool IsValidDirection(
            SurfaceEntityPosition currentPosition,
            SurfaceEntityPosition targetPosition
        )
        {
            if (currentPosition == null || targetPosition == null)
                return false;

            if (
                currentPosition.TryGetData(out DoorwayNodeData doorData)
                && doorData.IsConnected
                && doorData.ConnectedGridPosition == targetPosition.GridPosition
            )
                return true;

            var currentGridPosition = currentPosition.GridPosition;
            var targetGridPosition = targetPosition.GridPosition;
            bool sameDirection =
                currentPosition.SurfaceDirection == targetPosition.SurfaceDirection;
            bool slopesInvolved = currentPosition.IsSlope || targetPosition.IsSlope;

            if (slopesInvolved) // TODO: definitely not right, but good enough for now
                return true;

            bool isDiagonal =
                (
                    currentGridPosition.x != targetGridPosition.x
                    && currentGridPosition.y != targetGridPosition.y
                )
                || (
                    currentGridPosition.x != targetGridPosition.x
                    && currentGridPosition.z != targetGridPosition.z
                )
                || (
                    currentGridPosition.y != targetGridPosition.y
                    && currentGridPosition.z != targetGridPosition.z
                );

            if (!isDiagonal)
                return sameDirection || currentGridPosition == targetGridPosition;
            else
            {
                return CanWalkOnWalls
                    && currentPosition.SurfaceGridPosition == targetPosition.SurfaceGridPosition
                    && currentGridPosition.z == targetGridPosition.z;
            }
        }

        private bool IsValidGround(SurfaceEntityPosition position)
        {
            return position.IsSlope
                || position.SurfaceDirection == Direction.Down
                || CanWalkOnWalls;
        }

        private bool IsValidFall(EntityPosition currentPosition, EntityPosition targetPosition)
        {
            if (
                currentPosition.TryGetData(out DoorwayNodeData doorData)
                && doorData.IsConnected
                && doorData.ConnectedGridPosition == targetPosition.GridPosition
            )
                return true;

            var currentGridPosition = currentPosition.GridPosition;
            var targetGridPosition = targetPosition.GridPosition;
            var heightDifference = currentGridPosition.y - targetGridPosition.y;

            if (
                targetGridPosition.y > currentGridPosition.y
                || targetGridPosition.x != currentGridPosition.x
                || targetGridPosition.z != currentGridPosition.z
            )
                return false;
            else if (heightDifference <= 1)
                return true;
            else if (currentPosition is SurfaceEntityPosition surface)
                return surface.SurfaceDirection == Direction.Up;
            else
                return false;
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

        // TODO: replace with weight behaviour
        private int GetWeight(EntityPosition currentPosition, EntityPosition targetPosition)
        {
            return 0;
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
    }
}
