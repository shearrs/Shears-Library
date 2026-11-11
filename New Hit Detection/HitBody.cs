using System;
using System.Collections.Generic;
using Shears.Logging;
using UnityEngine;

namespace Shears.HitDetection
{
    public class HitBody : MonoBehaviour, ISHLoggable
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
        private bool fixedUpdate;

        [SerializeField]
        private bool isUnblockable;

        [SerializeField]
        private bool allowMultiHits;

        [SerializeField, ShowIf(nameof(allowMultiHits))]
        private float multiHitDelay;

        [SerializeField]
        private List<HitShape> shapes = new();

        [SerializeField]
        private List<HitBody> ignoreList = new();

        public event Action Enabled;
        public event Action Disabled;

        public void Enable()
        {
            if (enabled)
                return;

            enabled = true;
            Enabled?.Invoke();
        }

        public void Disable()
        {
            if (!enabled)
                return;

            enabled = false;
            Disabled?.Invoke();
        }

        private void Update()
        {
            if (fixedUpdate)
                return;

            DetectHits();
        }

        private void FixedUpdate()
        {
            if (!fixedUpdate)
                return;

            DetectHits();
        }

        public void DetectHits() { }
    }
}
