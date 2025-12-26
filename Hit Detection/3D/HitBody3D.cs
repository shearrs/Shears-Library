using Shears;
using Shears.Logging;
using System;
using System.Collections.Generic;
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
        [SerializeField, Tooltip("Optional provider for extra data to be sent with hits.")]
        private InterfaceReference<IHitDataProvider> dataProvider;

        [SerializeField, Tooltip("The shapes this HitBody3D uses to detect hits.")]
        private List<HitShape3D> shapes;

        [Header("Hit Settings")]
        [SerializeField, RuntimeReadonly, Tooltip("Whether or not this HitBody3D enables itself on Start.")]
        private bool enableOnStart = false;

        [SerializeField, Tooltip("Whether or not this HitBody3D updates in FixedUpdate.")]
        private bool fixedUpdate = false;

        [SerializeField, Tooltip("Whether or not this HitBody3D can repeatedly hit the same target without resetting.")]
        private bool multiHits;

        [SerializeField, Tooltip("The LayerMask that this HitBody3D can detect HurtBody3Ds on.")]
        protected LayerMask collisionMask = 1;

        [SerializeField, Tooltip("An optional list of HurtBody3Ds to ignore detection for.")]
        protected List<HurtBody3D> ignoreList;

        private bool isEnabled = false;
        private float enabledTime = 0.0f;
        private List<HurtBody3D> unclearedHits;
        private List<HurtBody3D> foundHurtbodies;
        private Dictionary<HurtBody3D, RaycastHit> finalHits;
        private List<int> sortedHits = new();

        public float EnabledTime => enabledTime;
        public List<HitShape3D> Shapes { get => shapes; set => shapes = value; }
        public bool UseFixedUpdate { get => fixedUpdate; set => fixedUpdate = value; }
        public bool MultiHits { get => multiHits; set => multiHits = value; }
        public LayerMask CollisionMask { get => collisionMask; set => collisionMask = value; }
        public List<HurtBody3D> IgnoreList { get => ignoreList; set => ignoreList = value; }

        public event Action Enabled;
        public event Action Disabled;
        public event Action<HitData3D> HitDelivered;
        #endregion

        #region Initialization
        private void Awake()
        {
            unclearedHits = ListPool<HurtBody3D>.Get();
            foundHurtbodies = ListPool<HurtBody3D>.Get();
            finalHits = DictionaryPool<HurtBody3D, RaycastHit>.Get();
            sortedHits = ListPool<int>.Get();
        }

        private void Start()
        {
            if (enableOnStart)
                Enable();
        }

        private void OnDestroy()
        {
            ListPool<HurtBody3D>.Release(unclearedHits);
            ListPool<HurtBody3D>.Release(foundHurtbodies);
            DictionaryPool<HurtBody3D, RaycastHit>.Release(finalHits);
            ListPool<int>.Release(sortedHits);
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

            enabledTime = 0.0f;
            isEnabled = false;
            Disabled?.Invoke();
        }
        #endregion

        private void Update()
        {
            if (!isEnabled)
                return;

            enabledTime += Time.deltaTime;

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

        private void DetectHits()
        {
            if (shapes.Count == 0)
            {
                this.Log("No shapes are assigned.", SHLogLevels.Warning);
                return;
            }

            finalHits.Clear();

            foreach (var shape in shapes)
                shape.Sweep(new(collisionMask, ValidateHits));

            DeliverHits();
        }

        private void ValidateHits(RaycastHit[] results, int hits, Comparison<int> sortFunc, out bool blocked)
        {
            sortedHits.Clear();

            for (int i = 0; i < hits; i++)
                sortedHits.Add(i);

            sortFunc ??= (h1, h2) => results[h1].distance.CompareTo(results[h2].distance);

            sortedHits.Sort(sortFunc);

            foreach (var hitIndex in sortedHits)
            {
                RaycastHit hit = results[hitIndex];
                
                if (hit.collider == null)
                {
                    this.Log($"Hit had no collider: {hit.transform.name}.", SHLogLevels.Verbose, context: hit.transform);
                    continue;
                }

                var hurtBody = GetHurtBodyForCollider(hit.collider);

                if (hurtBody == null)
                {
                    this.Log($"Hit had no HurtBody: {hit.transform.name}.", SHLogLevels.Verbose, context: hit.transform);
                    continue;
                }

                if (unclearedHits.Contains(hurtBody) && !multiHits)
                {
                    this.Log($"Hit was uncleared: {hurtBody.name}.", SHLogLevels.Verbose, context: hurtBody);
                    continue;
                }

                if (finalHits.TryGetValue(hurtBody, out var oldHit))
                {
                    if (oldHit.distance < hit.distance)
                        finalHits[hurtBody] = hit;
                }
                else
                    finalHits[hurtBody] = hit;

                if (hurtBody.IsBlocking)
                {
                    var blockHitData = new HitData3D(this, hurtBody, new(hit), dataProvider.Value?.GetData(), false);

                    if (hurtBody.CanBlock(blockHitData))
                    {
                        blocked = true;
                        return;
                    }
                }
            }

            blocked = false;
        }

        private void DeliverHits()
        {
            foreach (var (hurtBody, hit) in finalHits)
            {
                var subData = dataProvider.Value?.GetData();
                var hitData = new HitData3D(this, hurtBody, new(hit), subData, false);

                if (hurtBody.IsBlocking)
                    hitData = new HitData3D(this, hurtBody, new(hit), subData, hurtBody.CanBlock(hitData));

                hurtBody.OnHitReceived(hitData);
                OnHitDelivered(hitData);

                if (!multiHits && hitData.Blocked) // we don't store blocking as they can continue blocking
                    unclearedHits.Add(hurtBody);
            }
        }

        private HurtBody3D GetHurtBodyForCollider(Collider collider)
        {
            if (collider == null)
                return null;

            collider.transform.GetComponentsInParent(true, foundHurtbodies);

            foreach (var hurtBody in foundHurtbodies)
            {
                if (ignoreList.Contains(hurtBody))
                {
                    this.Log($"Ignoring HurtBody3D {hurtBody} due to ignore list.", SHLogLevels.Verbose);

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
            this.Log("HitBody3D delivered a hit.", SHLogLevels.Verbose);
            HitDelivered?.Invoke(data);
        }
    }
}