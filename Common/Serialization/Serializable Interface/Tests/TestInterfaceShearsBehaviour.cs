using UnityEngine;

namespace Shears
{
    [AddComponentMenu("GameObject/")]
    public class TestInterfaceShearsBehaviour : ShearsBehaviour, ITestInterface
    {
        public const string MESSAGE = "OEUF BEHAVIOUR!";

        [SerializeField]
        private ITestInterface _interface;

        public string GetMessage()
        {
            if (_interface != null)
                return _interface.GetMessage();
            else
                return MESSAGE;
        }
    }
}
