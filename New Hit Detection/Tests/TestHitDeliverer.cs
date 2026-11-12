using System;
using Shears.Logging;
using UnityEngine;

namespace Shears.HitDetection
{
    [RequireComponent(typeof(HitBody))]
    public partial class TestHitDeliverer : MonoBehaviour
    {
        [SerializeField]
        private TestDamageData[] damageData = Array.Empty<TestDamageData>();

        [Auto]
        [AutoEvent(nameof(HitBody.HitDelivered), nameof(OnHitDelivered))]
        private HitBody hitBody;

        private void Awake()
        {
            __AutoAwake();

            hitBody.Subdata = damageData;
        }

        private void OnHitDelivered(in HitData data)
        {
            SHLogger.Log("hit delivered!");
        }
    }
}
