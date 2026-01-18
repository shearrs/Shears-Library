using UnityEngine;

namespace Shears
{
    public class FrameSetter : MonoBehaviour
    {
        [SerializeField, Min(1)] private int targetFramerate = 120;

        private void Update()
        {
            Application.targetFrameRate = targetFramerate;
        }
    }
}
