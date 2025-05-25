using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public abstract class HitBody2D : MonoBehaviour, IHitBody
    {
        [Header("Hit Settings")]
        [SerializeField] private bool fixedUpdate = false;
        [SerializeField] private bool multiHits;
        [SerializeField] protected LayerMask collisionMask = 1;
        [SerializeField] protected List<Collider2D> ignoreList;

        private IHitDeliverer deliverer;
        private readonly List<IHitReceiver> unclearedHits = new(10);
        protected readonly Dictionary<Collider2D, RaycastHit2D> finalHits = new(10);

        public int ValidHitCount { get; private set; }
        public List<Collider2D> IgnoreList { get => ignoreList; set => ignoreList = value; }

        protected virtual void OnEnable()
        {
            ValidHitCount = 0;
            unclearedHits.Clear();
        }

        protected virtual void Start()
        {
            deliverer = GetComponentInParent<IHitDeliverer>();
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
                HurtBody2D hurtbody = GetHurtBodyForCollider(hit.collider, hit.collider.transform);

                if (hurtbody == null)
                    continue;

                IHitReceiver receiver = hurtbody.Receiver;

                if (multiHits || !unclearedHits.Contains(receiver))
                {
                    HitData2D hitData = new(deliverer, receiver, this, hurtbody, new(hit));

                    if (deliverer == null)
                    {
                        Debug.LogError("No deliverer found!");
                        return;
                    }
                    else if (receiver == null)
                    {
                        Debug.LogError("No receiver found!");
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
                    Debug.Log("ignore list: " + hurtbody.Collider.transform.name);
                    continue;
                }

                if (hurtbody.Collider == collider)
                    return hurtbody;
            }

            return null;
        }
    }
}
