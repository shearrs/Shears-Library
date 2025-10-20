using Shears.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Shears.HitDetection
{
    public abstract class HitBody3D : MonoBehaviour, IHitBody3D, ISHLoggable
    {
        [field: Header("Logging")]
        [field: SerializeField] public SHLogLevels LogLevels { get; set; } = SHLogLevels.Log | SHLogLevels.Issues;

        [Header("Hit Settings")]
        [SerializeField] private bool fixedUpdate = false;
        [SerializeField] private bool multiHits;
        [SerializeField] protected LayerMask collisionMask = 1;
        [SerializeField] protected List<Collider> ignoreList;

        private IHitDeliverer3D deliverer;
        private List<IHitReceiver<HitData3D>> unclearedHits;
        protected Dictionary<Collider, RaycastHit> finalHits;

        public IHitDeliverer3D Deliverer => deliverer;
        public int ValidHitCount { get; private set; }
        public List<Collider> IgnoreList { get => ignoreList; set => ignoreList = value; }
        public LayerMask CollisionMask { get => collisionMask; set => collisionMask = value; }

        IHitDeliverer<HitData3D> IHitBody<HitData3D>.Deliverer => Deliverer;

        public event Action Enabled;
        public event Action Disabled;

        protected virtual void Awake()
        {
            deliverer = GetComponentInParent<IHitDeliverer3D>();

            unclearedHits = ListPool<IHitReceiver<HitData3D>>.Get();
            finalHits = DictionaryPool<Collider, RaycastHit>.Get();
        }

        private void OnDestroy()
        {
            if (unclearedHits != null)
                ListPool<IHitReceiver<HitData3D>>.Release(unclearedHits);

            if (finalHits != null)
                DictionaryPool<Collider, RaycastHit>.Release(finalHits);
        }

        protected virtual void OnEnable()
        {
            ValidHitCount = 0;
            unclearedHits.Clear();

            Enabled?.Invoke();
        }

        protected virtual void OnDisable()
        {
            Disabled?.Invoke();
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
                if (deliverer == null)
                {
                    this.Log($"No deliverer for {this}!", SHLogLevels.Error);
                    return;
                }

                var hurtbody = GetHurtBodyForCollider(hit.collider, hit.collider.transform);

                if (hurtbody == null)
                    continue;

                IHitReceiver3D receiver = hurtbody.Receiver;

                if (receiver == null)
                {
                    this.Log($"No receiver found for {hurtbody}!", SHLogLevels.Error, context: hurtbody);
                    return;
                }

                if (multiHits || !unclearedHits.Contains(receiver))
                    SendHitEvents(hurtbody, hit);
            }
        }

        private void SendHitEvents(HurtBody3D body, RaycastHit hit)
        {
            var receiver = body.Receiver;
            HitData3D hitData = new(deliverer, receiver, this, body, new(hit), deliverer.GetCustomData());

            deliverer.OnHitDelivered(hitData);
            receiver.OnHitReceived(hitData);

            if (!multiHits)
                unclearedHits.Add(receiver);

            ValidHitCount++;
        }

        protected abstract void Sweep();

        protected HurtBody3D GetHurtBodyForCollider(Collider collider, Transform transform)
        {
            if (transform == null || collider == null)
                return null;

            var hurtbodies = transform.GetComponents<HurtBody3D>();

            foreach (HurtBody3D hurtbody in hurtbodies)
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