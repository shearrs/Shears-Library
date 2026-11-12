using System;
using System.Collections.Generic;
using System.Linq;
using Shears.Logging;
using UnityEngine;

namespace Shears.HitDetection
{
    public partial class HitBody : MonoBehaviour, ISHLoggable
    {
        [field: Header("Logging")]
        [field: SerializeField]
        public SHLogLevels LogLevels { get; set; } = SHLogUtil.Default;

        [Header("Hit Body")]
        [SerializeField]
        private GameObject owner;

        [SerializeField]
        private LayerMask collisionMask = -1;

        [SerializeField]
        private UpdateMode updateMode;

        [SerializeField]
        private bool isUnblockable;

        [SerializeField]
        private bool allowMultiHits;

        [SerializeField, ShowIf(nameof(allowMultiHits))]
        private float multiHitDelay = 0.15f;

        [SerializeField]
        private List<HitShape> shapes = new();

        [SerializeField]
        private List<HurtBody> ignoreList = new();

        [AutoEvent(nameof(Timer.Completed), nameof(ClearHits))]
        private readonly Timer multiHitTimer = new();
        private readonly List<HurtBody> unclearedHits = new();
        private readonly List<HurtBody> foundBodies = new();
        private readonly Dictionary<HurtBody, MappedHit> finalHits = new();

        public HitSubdata[] Subdata { get; set; } = Array.Empty<HitSubdata>();

        public event Action Enabled;
        public event Action Disabled;
        public event HitEvent HitDelivered;
        public event HitEvent BlockedHitDelivered;

        public enum UpdateMode
        {
            Update,
            FixedUpdate,
            Manual,
        }

        private readonly struct MappedHit
        {
            public HitShape SourceShape { get; }
            public Vector3 HitPoint { get; }
            public Vector3 HitNormal { get; }
            public bool IsBlocked { get; }

            public MappedHit(
                HitShape sourceShape,
                Vector3 hitPoint,
                Vector3 hitNormal,
                bool isBlocked
            )
            {
                SourceShape = sourceShape;
                HitPoint = hitPoint;
                HitNormal = hitNormal;
                IsBlocked = isBlocked;
            }
        }

        private void Update()
        {
            if (updateMode != UpdateMode.Update)
                return;

            DetectHits();
        }

        private void FixedUpdate()
        {
            if (updateMode != UpdateMode.FixedUpdate)
                return;

            DetectHits();
        }

        [ContextMenu("Enable")]
        public void Enable()
        {
            if (enabled)
                return;

            ClearHits();

            enabled = true;
            Enabled?.Invoke();
        }

        [ContextMenu("Disable")]
        public void Disable()
        {
            if (!enabled)
                return;

            ClearHits();

            enabled = false;
            Disabled?.Invoke();
        }

        public void DetectHits()
        {
            var sweepContext = new SweepContext(collisionMask, isUnblockable);
            finalHits.Clear();

            foreach (var shape in shapes)
            {
                var sweepResult = shape.Sweep(sweepContext);
                int? currentID = null;
                bool isBlocked = false;

                foreach (var hit in sweepResult.Hits)
                {
                    if (!currentID.HasValue)
                        currentID = hit.ResultID;

                    if (currentID != hit.ResultID)
                    {
                        isBlocked = false;
                        currentID = hit.ResultID;
                    }

                    if (isBlocked)
                        continue;

                    if (!TryFindHurtBody(hit.Collider, out var hurtBody))
                    {
                        this.LogVerbose(
                            $"Hit had no {nameof(HurtBody)}: {hit.Collider.name}.",
                            hit.Collider
                        );
                        continue;
                    }

                    if (unclearedHits.Contains(hurtBody))
                    {
                        this.LogVerbose($"Hit was uncleared: {hurtBody.name}.", hurtBody);
                        continue;
                    }

                    var hitData = new HitData(shape, hit.HitPoint, hit.HitNormal, Subdata);

                    if (!isUnblockable && hurtBody.CanBlock(hitData))
                    {
                        isBlocked = true;
                        this.LogVerbose($"Hit was blocked: {shape.name}.", shape);
                    }

                    if (!finalHits.ContainsKey(hurtBody))
                        finalHits[hurtBody] = new(shape, hit.HitPoint, hit.HitNormal, isBlocked);
                }

                sweepResult.Dispose();
            }

            DeliverHits();
        }

        public void ClearHits()
        {
            unclearedHits.Clear();
            multiHitTimer.Stop();
        }

        private void DeliverHits()
        {
            foreach (var (hurtBody, hit) in finalHits)
            {
                var hitData = new HitData(hit.SourceShape, hit.HitPoint, hit.HitNormal, Subdata);

                if (hit.IsBlocked)
                {
                    hurtBody.BlockHit(hitData);
                    BlockedHitDelivered?.Invoke(hitData);
                }
                else
                {
                    hurtBody.Hit(hitData);
                    unclearedHits.Add(hurtBody);

                    if (allowMultiHits && multiHitTimer.IsDone)
                        multiHitTimer.Start(multiHitDelay);

                    HitDelivered?.Invoke(hitData);
                }
            }
        }

        private bool TryFindHurtBody(Collider collider, out HurtBody body)
        {
            if (collider == null)
            {
                body = null;
                return false;
            }

            collider.transform.GetComponentsInParent(false, foundBodies);

            foreach (var hurtBody in foundBodies)
            {
                if (ignoreList.Contains(hurtBody))
                {
                    this.LogVerbose($"Ignoring {nameof(HurtBody)} due to ignore list.", hurtBody);

                    if (hurtBody.Colliders.Contains(collider))
                    {
                        body = null;
                        return false;
                    }

                    continue;
                }

                if (hurtBody.Colliders.Contains(collider))
                {
                    body = hurtBody;
                    return true;
                }
            }

            body = null;
            return false;
        }
    }
}
