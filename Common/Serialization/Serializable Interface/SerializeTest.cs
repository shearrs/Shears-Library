using UnityEngine;

namespace Shears
{
    public class SerializeTest : MonoBehaviour, IInterfaceSerializer
    {
        [SerializeField]
        private MonoBehaviour normalField;

        [SerializeField]
        private ITestInterface firstTest;

        [SerializeField]
        private ITestInterface secondTest;

        [SerializeField]
        private bool normalToggle;

        [SerializeField]
        private InterfaceDictionary __interfaceEntries = new();

        InterfaceDictionary IInterfaceSerializer.InterfaceEntries => __interfaceEntries;

        [ContextMenu("Test")]
        private void Test()
        {
            var newTest = new TestInterfaceRaw { Message = "Runtime message!" };
            secondTest = newTest;
            secondTest.LogMessage();
        }

        private void Awake()
        {
            firstTest.LogMessage();
        }
    }
}
