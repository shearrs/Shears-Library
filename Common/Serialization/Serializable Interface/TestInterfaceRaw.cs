using UnityEngine;

namespace Shears
{
    [System.Serializable]
    public class TestInterfaceRaw : ITestInterface, IInterfaceSerializable
    {
        [SerializeField]
        private string message;

        [SerializeField]
        private ITestInterface nestedTest;

        [SerializeReference]
        private InterfaceDictionary __interfaceEntries = new();

        InterfaceDictionary IInterfaceSerializable.InterfaceEntries => __interfaceEntries;

        public string Message
        {
            get => message;
            set => message = value;
        }

        public void LogMessage()
        {
            nestedTest.LogMessage();
        }

        public override string ToString()
        {
            return $"nested: {nestedTest}";
        }
    }
}
