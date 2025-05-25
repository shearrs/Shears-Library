using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public abstract class HitBody3D : MonoBehaviour, IHitBody
    {
        [Header("Hit Settings")]
        [SerializeField] private bool fixedUpdate = false;
        [SerializeField] private bool multiHits;
        [SerializeField] protected LayerMask collisionMask = 1;
        [SerializeField] protected List<Collider> ignoreList;

        private IHitDeliverer deliverer;
        private readonly List<IHitReceiver> unclearedHits = new(10);
        protected readonly Dictionary<Collider, RaycastHit> finalHits = new(10);

        public int ValidHitCount { get; private set; }
        public List<Collider> IgnoreList { get => ignoreList; set => ignoreList = value; }

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

            foreach (RaycastHit hit in finalHits.Values)
            {
                HurtBody3D hurtbody = GetHurtBodyForCollider(hit.collider, hit.collider.transform);

                if (hurtbody == null)
                    continue;

                IHitReceiver receiver = hurtbody.Receiver;

                if (multiHits || !unclearedHits.Contains(receiver))
                {
                    HitData3D hitData = new(deliverer, receiver, this, hurtbody, new(hit));

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

        private HurtBody3D GetHurtBodyForCollider(Collider collider, Transform transform)
        {
            if (transform == null || collider == null)
                return null;

            HurtBody3D[] hurtbodies = transform.GetComponents<HurtBody3D>();

            foreach (HurtBody3D hurtbody in hurtbodies)
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