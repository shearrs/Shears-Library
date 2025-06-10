using Shears.Logging;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shears.HitDetection
{
    public abstract class HitBody2D : MonoBehaviour, IHitBody2D, ISHLoggable
    {
        [field: Header("Logging")]
        [field: SerializeField] public SHLogLevels LogLevels { get; set; } = SHLogLevels.Issues;

        [Header("Hit Settings")]
        [SerializeField] private bool fixedUpdate = false;
        [SerializeField] private bool oneHitPerFrame = false;
        [SerializeField] private bool multiHits;
        [SerializeField] protected LayerMask collisionMask = 1;
        [SerializeField] protected List<Collider2D> ignoreList;

        private IHitDeliverer2D deliverer;
        private readonly List<IHitReceiver2D> unclearedHits = new(10);
        protected readonly Dictionary<Collider2D, RaycastHit2D> finalHits = new(10);

        public IHitDeliverer2D Deliverer => deliverer;
        public int ValidHitCount { get; private set; }
        public List<Collider2D> IgnoreList { get => ignoreList; set => ignoreList = value; }

        IHitDeliverer<HitData2D> IHitBody<HitData2D>.Deliverer => Deliverer;

        protected virtual void OnEnable()
        {
            ValidHitCount = 0;
            unclearedHits.Clear();
        }

        private void Awake()
        {
            deliverer = GetComponentInParent<IHitDeliverer2D>();
        }

        private void Update()
        {
            if (!fixedUpdate)
                CheckForHits();
        }

        private void FixedUpdate()
        {
            if (fixedUpdate)
                CheckForHits();
        }

        private void CheckForHits()
        {
            finalHits.Clear();
            Sweep();

            var hits = finalHits.Values.OrderBy(hit => hit.distance);

            foreach (RaycastHit2D hit in hits)
            {
                if (deliverer == null)
                {
                    this.Log($"No deliverer for {this}!", SHLogLevels.Warning);
                    return;
                }

                var hurtbody = GetHurtBodyForCollider(hit.collider, hit.collider.transform);

                if (hurtbody == null)
                {
                    if (!IgnoreList.Contains(hit.collider))
                        this.Log($"No HurtBody found for {hit.collider}!", SHLogLevels.Warning);

                    continue;
                }

                IHitReceiver2D receiver = hurtbody.Receiver;

                if (receiver == null)
                {
                    this.Log("No receiver found!", SHLogLevels.Warning, context: hit.collider);
                    return;
                }

                if (receiver is IHitBlocker2D blocker && blocker.IsBlocking)
                {
                    this.Log($"Hit blocker {blocker.Transform}.");
                    break;
                }

                if (multiHits || !unclearedHits.Contains(receiver))
                {
                    HitData2D hitData = new(deliverer, receiver, this, hurtbody, new(hit), deliverer.GetCustomData());

                    deliverer.OnHitDelivered(hitData);
                    receiver.OnHitReceived(hitData);

                    this.Log($"Delivered hit to {receiver}!");

                    if (!multiHits)
                        unclearedHits.Add(receiver);

                    ValidHitCount++;

                    if (oneHitPerFrame)
                        break;
                }
            }
        }

        protected abstract void Sweep();

        private HurtBody2D GetHurtBodyForCollider(Collider2D collider, Transform transform)
        {
            if (transform == null || collider == null)
                return null;

            var hurtbodies = transform.GetComponents<HurtBody2D>();

            foreach (HurtBody2D hurtbody in hurtbodies)
            {
                if (ignoreList.Contains(hurtbody.Collider))
                {
                    this.Log("Ignore List object detected: " + hurtbody.Collider.transform.name, SHLogLevels.Verbose);

                    continue;
                }

                if (hurtbody.Collider == collider)
                    return hurtbody;
            }

            return null;
        }
    }
}
