using UnityEngine;

namespace Shears
{
    [System.Serializable]
    public class TestInterfaceRaw : ITestInterface, IInterfaceSerializable
    {
        public const string MESSAGE = "Raw message!";

        [SerializeField]
        private ITestInterface _nestedTest;

        [SerializeReference]
        private InterfaceDictionary __interfaceEntries = new();

        public ITestInterface NestedField
        {
            get => _nestedTest;
            set => _nestedTest = value;
        }

        InterfaceDictionary IInterfaceSerializable.InterfaceEntries => __interfaceEntries;

        public string GetMessage()
        {
            if (_nestedTest != null)
                return _nestedTest.GetMessage();
            else
                return MESSAGE;
        }
    }
}
