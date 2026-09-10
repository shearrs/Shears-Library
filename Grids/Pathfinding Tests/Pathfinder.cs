using System.Collections.Generic;
using UnityEngine;
using static Shears.Grids.EntityPosition;

namespace Shears.Grids
{
    public class Pathfinder : ShearsBehaviour
    {
        private const int CARDINAL_COST = 10;
        private const int DIAGONAL_COST = 14;

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

        public void CalculatePath(Vector3 start, Vector3 target)
        {
            if (grid == null)
            {
                LogError("Grid is null!");
                return;
            }

            if (!grid.TryGetPosition(start, out var startPosition))
            {
                LogError($"Could not find starting position at: {start}.");
                return;
            }

            if (!grid.TryGetPosition(target, out var targetPosition))
            {
                LogError($"Could not find target position at: {target}.");
                return;
            }

            CalculatePath(startPosition, targetPosition);
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
                        grid.TryGetSurfacePosition(
                            doorData.ConnectedGridPosition,
                            out var doorPosition
                        )
                    )
                        neighbors.Add(doorPosition);
                }

                foreach (var neighbor in neighbors)
                {
                    if (
                        neighbor == null
                        || closedSet.Contains(neighbor)
                        || !IsValidMove(currentPosition, neighbor)
                    )
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
                return entity.CanWalkOnWalls
                    && currentPosition.SurfaceGridPosition == targetPosition.SurfaceGridPosition
                    && currentGridPosition.z == targetGridPosition.z;
            }
        }

        private bool IsValidGround(SurfaceEntityPosition position)
        {
            return position.IsSlope
                || position.SurfaceDirection == Direction.Down
                || entity.CanWalkOnWalls;
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

                // need the ability to get the current position of the entity
                // CalculatePath(currentPosition, currentTarget);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos || grid == null || entityPath == null || path == null)
                return;

            Gizmos.color = Color.magenta;

            if (grid.TryGetPosition(entity.Position, out var position))
                GizmosUtil.DrawWireDisc(position.WorldPosition, Vector3.up, 0.25f);

            if (entityPath.Count == 0 || path.Count == 0)
                return;

            Gizmos.color = Color.green;
            GizmosUtil.DrawWireDisc(entityPath[^1].WorldPosition, Vector3.up, 0.25f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(entityPath[^1].WorldPosition, position.WorldPosition);

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

                Gizmos.DrawLine(nextPosition.Position, current.Position);

                if (openSetMap.TryGetValue(current.EntityPosition, out var entry))
                {
                    int weight =
                        entry.FCost
                        + GetWeight(current.EntityPosition, nextPosition.EntityPosition);
                    GizmosUtil.DrawText(current.Position, weight);
                }
            }
        }
    }
}
