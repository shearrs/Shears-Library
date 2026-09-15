using UnityEngine;

namespace Shears
{
    [AddComponentMenu("GameObject/")]
    public class TestInterfaceMonobehaviour : MonoBehaviour, ITestInterface
    {
        public const string MESSAGE = "Monobehaviour message!";

        public string GetMessage()
        {
            return MESSAGE;
        }
    }
}
