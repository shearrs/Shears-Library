using UnityEngine;

namespace Shears.HitDetection
{
    public class TestHitReceiver : MonoBehaviour, IHitReceiver
    {
        public Transform Transform => transform;

        public void OnHitReceived(IHitData hitData)
        {
            Debug.Log("hit received!");
        }
    }
}