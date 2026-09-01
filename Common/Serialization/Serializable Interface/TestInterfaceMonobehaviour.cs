using UnityEngine;

namespace Shears
{
    public class TestInterfaceMonobehaviour : MonoBehaviour, ITestInterface
    {
        public void LogMessage()
        {
            print("Hello Interface!");
        }
    }
}
