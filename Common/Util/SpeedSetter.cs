using UnityEngine;

namespace Shears
{
    public class SpeedSetter : MonoBehaviour
    {
        [SerializeField, Range(0, 10)]
        private float timeScale = 1.0f;

        private void Update()
        {
            Time.timeScale = timeScale;
        }
    }
}
