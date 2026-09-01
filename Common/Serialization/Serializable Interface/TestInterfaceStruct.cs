using UnityEngine;

namespace Shears
{
    [System.Serializable]
    public struct TestInterfaceStruct : ITestInterface
    {
        [SerializeField]
        private string _structString;

        public readonly void LogMessage()
        {
            Debug.Log(_structString);
        }
    }
}
