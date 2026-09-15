using UnityEngine;

namespace Shears
{
    [System.Serializable]
    public struct TestInterfaceStruct : ITestInterface
    {
        [SerializeField]
        private string _message;

        public string Message
        {
            readonly get => _message;
            set => _message = value;
        }

        public readonly string GetMessage()
        {
            return _message;
        }
    }
}
