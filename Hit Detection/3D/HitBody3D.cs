using System;
using System.Collections.Generic;
using Shears;
using Shears.Logging;
using UnityEngine;
using UnityEngine.Pool;

namespace Shears.HitDetection
{
    public class HitBody3D : MonoBehaviour, ISHLoggable
    {
        #region Variables
        [field: Header("Logging")]
        [field: SerializeField]
        public SHLogLevels LogLevels { get; set; } = SHLogLevels.Log | SHLogUtil.Issues;

        [Header("Components")]
        [
            SerializeField,
            Tooltip(
                "The owner of this hit body. Only exists for logic that would want to know this."
            )
        ]
        private GameObject owner;

        [SerializeField, Tooltip("Optional provider for extra data to be sent with hits.")]
        private InterfaceReference<IHitDataProvider> dataProvider;

        [SerializeField, Tooltip("The shapes this HitBody3D uses to detect hits.")]
        private List<HitShape3D> shapes;

        [Header("Hit Settings")]
        [
            SerializeField,
            RuntimeReadOnly,
            Tooltip("Whether or not this HitBody3D enables itself on Start.")
        ]
        private bool enableOnStart = false;

        [SerializeField, Tooltip("Whether or not this HitBody3D updates in FixedUpdate.")]
        private bool fixedUpdate = false;

        [
            SerializeField,
            Tooltip(
                "Whether or not this HitBody3D can repeatedly hit the same target without resetting."
            )
        ]
        private bool multiHits;

        [SerializeField, Tooltip("Whether or not this HitBody3D is unblockable.")]
        private bool unblockable = false;

        [SerializeField, Tooltip("The LayerMask that this HitBody3D can detect HurtBody3Ds on.")]
        protected LayerMask collisionMask = 1;

        [SerializeField, Tooltip("An optional list of HurtBody3Ds to ignore detection for.")]
        protected List<HurtBody3D> ignoreList;

        private bool isEnabled = false;
        private List<HurtBody3D> unclearedHits;
        private List<HurtBody3D> foundHurtbodies;
        private Dictionary<HurtBody3D, MappedHit> finalHits;
        private List<int> sortedHits;

        public GameObject Owner
        {
            get
            {
                if (owner == null && !Application.isPlaying)
                    owner = gameObject;

                return owner;
            }
            set => owner = value;
        }
        public bool IsEnabled => isEnabled;
        public List<HitShape3D> Shapes
        {
            get => shapes;
            set => shapes = value;
        }
        public bool UseFixedUpdate
        {
            get => fixedUpdate;
            set => fixedUpdate = value;
        }
        public bool MultiHits
        {
            get => multiHits;
            set => multiHits = value;
        }
        public bool Unblockable
        {
            get => unblockable;
            set => unblockable = value;
        }
        public LayerMask CollisionMask
        {
            get => collisionMask;
            set => collisionMask = value;
        }
        public List<HurtBody3D> IgnoreList
        {
            get => ignoreList;
            set => ignoreList = value;
        }

        public event Action Enabled;
        public event Action Disabled;
        public event Action<HitData3D> HitDelivered;
        #endregion

        private readonly struct MappedHit
        {
            public readonly HitShape3D shape;
            public readonly HitResult3D hit;

            public MappedHit(HitShape3D shape, HitResult3D hit)
            {
                this.shape = shape;
                this.hit = hit;
            }
        }

        #region Initialization
        private void Awake()
        {
            CollectionUtil.GetPooled(out unclearedHits);
            CollectionUtil.GetPooled(out foundHurtbodies);
            CollectionUtil.GetPooled(out finalHits);
            CollectionUtil.GetPooled(out sortedHits);

            for (int i = 0; i < shapes.Count; i++)
                shapes[i].Body = this;
        }

        private void Start()
        {
            if (enableOnStart)
                Enable();
        }

        private void OnDestroy()
        {
            if (unclearedHits == null)
                return;

            CollectionUtil.ReleasePooled(unclearedHits);
            CollectionUtil.ReleasePooled(foundHurtbodies);
            CollectionUtil.ReleasePooled(finalHits);
            CollectionUtil.ReleasePooled(sortedHits);
        }

        public void Enable()
        {
            if (isEnabled)
                return;

            unclearedHits.Clear();
            isEnabled = true;
            Enabled?.Invoke();
        }

        public void Disable()
        {
            if (!isEnabled)
                return;

            isEnabled = false;
            Disabled?.Invoke();
        }

        public void ClearHits()
        {
            unclearedHits.Clear();
        }
        #endregion

        private void Update()
        {
            if (!isEnabled)
                return;

            if (fixedUpdate)
                return;

            DetectHits();
        }

        private void FixedUpdate()
        {
            if (!isEnabled || !fixedUpdate)
                return;

            DetectHits();
        }

        public void DetectHits()
        {
            if (shapes.Count == 0)
            {
                this.LogWarning("No shapes are assigned.");
                return;
            }

            finalHits.Clear();

            foreach (var shape in shapes)
            {
                shape.Body = this;
                shape.Sweep(new(collisionMask, ValidateHits));
            }

            DeliverHits();
        }

        private void ValidateHits(
            HitShape3D shape,
            HitResult3D[] results,
            int hits,
            Comparison<int> sortFunc,
            out bool blocked
        )
        {
            sortedHits.Clear();

            for (int i = 0; i < hits; i++)
                sortedHits.Add(i);

            sortFunc ??= (h1, h2) => results[h1].distance.CompareTo(results[h2].distance);

            sortedHits.Sort(sortFunc);

            foreach (var hitIndex in sortedHits)
            {
                var hit = results[hitIndex];

                if (hit.collider == null)
                {
                    this.LogVerbose($"Hit had no collider: {hit.transform.name}.", hit.transform);
                    continue;
                }

                var hurtBody = GetHurtBodyForCollider(hit.collider);

                if (hurtBody == null)
                {
                    this.LogVerbose($"Hit had no HurtBody: {hit.transform.name}.", hit.transform);
                    continue;
                }

                if (unclearedHits.Contains(hurtBody) && !multiHits)
                {
                    this.LogVerbose($"Hit was uncleared: {hurtBody.name}.", hurtBody);
                    continue;
                }

                if (!finalHits.TryGetValue(hurtBody, out var oldHit))
                    finalHits[hurtBody] = new(shape, hit);

                if (!unblockable && hurtBody.IsBlocking)
                {
                    var blockHitData = new HitData3D(
                        shape,
                        this,
                        hurtBody,
                        hit,
                        dataProvider.Value?.GetData(),
                        false
                    );

                    if (hurtBody.CanBlock(blockHitData))
                    {
                        this.LogVerbose($"Hit was blocked: {shape.name}.", shape);
                        blocked = true;
                        return;
                    }
                }
            }

            blocked = false;
        }

        private void DeliverHits()
        {
            foreach (var (hurtBody, hitMap) in finalHits)
            {
                var subData = dataProvider.Value?.GetData();
                var hitData = new HitData3D(
                    hitMap.shape,
                    this,
                    hurtBody,
                    hitMap.hit,
                    subData,
                    false
                );

                if (!unblockable && hurtBody.IsBlocking)
                    hitData = new HitData3D(
                        hitMap.shape,
                        this,
                        hurtBody,
                        hitMap.hit,
                        subData,
                        hurtBody.CanBlock(hitData)
                    );

                hurtBody.OnHitReceived(hitData);
                OnHitDelivered(hitData);

                if (!multiHits && !hitData.Blocked) // we don't store blocking as they can continue blocking
                    unclearedHits.Add(hurtBody);

                if (!isEnabled)
                    break;
            }
        }

        private HurtBody3D GetHurtBodyForCollider(Collider collider)
        {
            if (collider == null)
                return null;

            collider.transform.GetComponentsInParent(false, foundHurtbodies);

            foreach (var hurtBody in foundHurtbodies)
            {
                if (ignoreList.Contains(hurtBody))
                {
                    this.LogVerbose($"Ignoring HurtBody3D {hurtBody} due to ignore list.");

                    if (hurtBody.Colliders.Contains(collider))
                        return null;

                    continue;
                }

                if (hurtBody.Colliders.Contains(collider))
                    return hurtBody;
            }

            return null;
        }

        internal void OnHitDelivered(HitData3D data)
        {
            this.LogVerbose("HitBody3D delivered a hit.");
            HitDelivered?.Invoke(data);
        }
    }
}
