using UnityEngine;

namespace Shears
{
    [System.Serializable]
    public class TestInterfaceRaw : ITestInterface, IInterfaceSerializer
    {
        [SerializeField]
        private string message;

        [SerializeField]
        private ITestInterface nestedTest;

        [SerializeReference]
        private InterfaceDictionary __interfaceEntries = new();

        InterfaceDictionary IInterfaceSerializer.InterfaceEntries => __interfaceEntries;

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
