using UnityEngine;

namespace Shears.HitDetection
{
    [System.Serializable]
    public class TestDamageData : HitSubdata
    {
        [field: SerializeField]
        public int Damage { get; private set; } = 1;

        public TestDamageData(int damage)
        {
            Damage = damage;
        }
    }
}
