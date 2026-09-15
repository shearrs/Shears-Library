using UnityEngine;

namespace Shears
{
    [AddComponentMenu("GameObject/")]
    public class SerializeTest : ShearsBehaviour
    {
        [SerializeField]
        private ITestInterface _firstField;

        [SerializeField]
        private ITestInterface _secondField;

        [SerializeField]
        private ITestInterface _thirdField;

        public ITestInterface FirstField
        {
            get => _firstField;
            set => _firstField = value;
        }

        public ITestInterface SecondField
        {
            get => _secondField;
            set => _secondField = value;
        }

        public ITestInterface ThirdField
        {
            get => _thirdField;
            set => _thirdField = value;
        }
    }
}
