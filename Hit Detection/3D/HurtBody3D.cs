using Shears.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public class HurtBody3D : MonoBehaviour, ISHLoggable
    {
        [field: Header("Logging")]
        [field: SerializeField]
        public SHLogLevels LogLevels { get; set; } = SHLogLevels.Log | SHLogUtil.Issues;

        [Header("Settings")]
        [SerializeField, Tooltip("Whether or not this HurtBody3D blocks hits.")]
        private bool isBlocking = false;

        [SerializeField, Tooltip("An optional provider for logic on when to block hits."), ShowIf(nameof(isBlocking))]
        private InterfaceReference<IBlockProvider> blockProvider;

        [SerializeField, Tooltip("The colliders for receiving hits.")]
        private List<Collider> colliders;

        public bool IsBlocking { get => isBlocking; set => isBlocking = value; }
        public IBlockProvider BlockProvider { get => blockProvider.Value; set => blockProvider.Value = value; }
        public List<Collider> Colliders { get => colliders; set => colliders = value; }

        public event Action<HitData3D> HitReceived;

        public bool CanBlock(HitData3D data)
        {
            if (!isBlocking)
                return false;
            else if (blockProvider.Value == null)
                return true;
            else
                return blockProvider.Value.CanBlock(data);
        }

        internal void OnHitReceived(HitData3D data)
        {
            this.Log("HurtBody3D received a hit.", SHLogLevels.Verbose);
            HitReceived?.Invoke(data);
        }
    }
}
