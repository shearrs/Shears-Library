using UnityEngine;

namespace Shears.Logging
{
    public class TestLogger : MonoBehaviour
    {
        public void Log(string message)
        {
            SHLogger.Log(message);
        }
    }
}
