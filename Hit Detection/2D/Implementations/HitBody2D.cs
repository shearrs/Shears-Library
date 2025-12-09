using Shears.Logging;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shears.HitDetection
{
    public abstract class HitBody2D : MonoBehaviour, IHitBody2D, ISHLoggable
    {
        #region Variables
        [field: Header("Logging")]
        [field: SerializeField] public SHLogLevels LogLevels { get; set; } = SHLogLevels.Log | SHLogUtil.Issues;

        [Header("Hit Settings")]
        [SerializeField] private bool fixedUpdate = false;
        [SerializeField] private bool oneHitPerFrame = false;
        [SerializeField] private bool multiHits = false;
        [SerializeField] private bool unblockable = false;
        [SerializeField] protected LayerMask collisionMask = 1;
        [SerializeField] protected List<Collider2D> ignoreList;

        private IHitDeliverer2D deliverer;
        private readonly List<IHitReceiver2D> unclearedHits = new(16);
        protected readonly List<HitRay2D> hitRays = new();

        public IHitDeliverer2D Deliverer => deliverer;
        public int ValidHitCount { get; private set; }
        public List<Collider2D> IgnoreList { get => ignoreList; set => ignoreList = value; }

        IHitDeliverer<HitData2D> IHitBody<HitData2D>.Deliverer => Deliverer;
        #endregion

        protected virtual void OnEnable()
        {
            ValidHitCount = 0;
            unclearedHits.Clear();
        }

        protected virtual void Awake()
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
            Sweep();

            foreach (var hitRay in hitRays)
            {
                var hits = hitRay.ValidHits.Values.OrderBy(hit => hit.distance);

                foreach (var hit in hits)
                {
                    if (deliverer == null)
                    {
                        this.Log($"No deliverer for {this}!", SHLogLevels.Warning);
                        return;
                    }

                    var hurtbody = GetHurtBodyForCollider(hit.collider, hit.collider.transform);

                    if (hurtbody == null)
                        continue;

                    IHitReceiver2D receiver = hurtbody.Receiver;

                    if (receiver == null)
                    {
                        this.Log("No receiver found!", SHLogLevels.Warning, context: hit.collider);
                        return;
                    }

                    if (!unblockable && receiver is IHitBlocker2D blocker && blocker.IsBlocking)
                    {
                        var hitData = new HitData2D(deliverer, receiver, this, hurtbody, new(hit), deliverer.GetCustomData());

                        deliverer.OnHitBlocked(hitData);
                        blocker.OnHitBlocked(hitData);

                        this.Log($"Hit blocked by {blocker}.");

                        break;
                    }

                    if (multiHits || !unclearedHits.Contains(receiver))
                    {
                        var hitData = new HitData2D(deliverer, receiver, this, hurtbody, new(hit), deliverer.GetCustomData());

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
