using Shears.Logging;
using System.Collections.Generic;
using UnityEngine;

using HitData2D = Shears.HitDetection.HitData<Shears.HitDetection.HitResult2D>;

namespace Shears.HitDetection
{
    public abstract class HitBody2D : MonoBehaviour, IHitBody<HitData2D>, ISHLoggable
    {
        [field: Header("Logging")]
        [field: SerializeField] public SHLogLevel LogLevels { get; set; } = SHLogLevel.Everything;

        [Header("Hit Settings")]
        [SerializeField] private bool fixedUpdate = false;
        [SerializeField] private bool multiHits;
        [SerializeField] protected LayerMask collisionMask = 1;
        [SerializeField] protected List<Collider2D> ignoreList;

        private IHitDeliverer<HitData2D> deliverer;
        private readonly List<IHitReceiver<HitData2D>> unclearedHits = new(10);
        protected readonly Dictionary<Collider2D, RaycastHit2D> finalHits = new(10);

        public IHitDeliverer<HitData2D> Deliverer => deliverer;
        public int ValidHitCount { get; private set; }
        public List<Collider2D> IgnoreList { get => ignoreList; set => ignoreList = value; }

        protected virtual void OnEnable()
        {
            ValidHitCount = 0;
            unclearedHits.Clear();
        }

        private void Awake()
        {
            deliverer = GetComponentInParent<IHitDeliverer<HitData2D>>();
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

            foreach (RaycastHit2D hit in finalHits.Values)
            {
                IHurtBody<HitData2D> hurtbody = GetHurtBodyForCollider(hit.collider, hit.collider.transform);

                if (hurtbody == null)
                    continue;

                IHitReceiver<HitData2D> receiver = hurtbody.Receiver;
                
                if (multiHits || !unclearedHits.Contains(receiver))
                {
                    HitData2D hitData = new(deliverer, receiver, this, hurtbody, new(hit), deliverer.GetCustomData());

                    if (deliverer == null)
                    {
                        this.Log("No deliverer found!", SHLogLevel.Error);
                        return;
                    }
                    else if (receiver == null)
                    {
                        this.Log("No receiver found!", SHLogLevel.Error);
                        return;
                    }

                    deliverer.OnHitDelivered(hitData);
                    receiver.OnHitReceived(hitData);

                    if (!multiHits)
                        unclearedHits.Add(receiver);

                    ValidHitCount++;
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
                    this.Log("Ignore List object detected: " + hurtbody.Collider.transform.name, SHLogLevel.Verbose);
                    continue;
                }

                if (hurtbody.Collider == collider)
                    return hurtbody;
            }

            return null;
        }
    }
}
