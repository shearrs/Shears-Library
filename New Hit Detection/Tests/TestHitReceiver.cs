using Shears.Logging;
using UnityEngine;

namespace Shears.HitDetection
{
    [RequireComponent(typeof(HurtBody))]
    public partial class TestHitReceiver : MonoBehaviour
    {
        [Auto]
        [AutoEvent(nameof(HurtBody.HitReceived), nameof(OnHitReceived))]
        private HurtBody hurtBody;

        private void OnHitReceived(in HitData data)
        {
            if (data.TryGetData(out TestDamageData damageData))
                SHLogger.Log("damage: " + damageData.Damage);

            SHLogger.Log("hit received");
        }
    }
}
