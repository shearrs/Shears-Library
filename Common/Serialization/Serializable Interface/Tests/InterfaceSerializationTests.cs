using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Shears
{
    public class InterfaceSerializationTests
    {
        private SerializeTest testObject;

        [SetUp]
        public void SetUp()
        {
            var gameObject = new GameObject("Interface Serialization Test");
            testObject = gameObject.AddComponent<SerializeTest>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(testObject.gameObject);
        }

        [UnityTest]
        public IEnumerator SetMonobehaviourValue()
        {
            var gameObject = new GameObject("Monobehaviour Interface Value");
            var monoValue = gameObject.AddComponent<TestInterfaceMonobehaviour>();
            gameObject.transform.SetParent(testObject.transform);

            testObject.FirstField = monoValue;

            Assert.IsNotNull(testObject.FirstField);

            yield return null;

            Assert.IsNotNull(testObject.FirstField);

            var message = testObject.FirstField.GetMessage();

            Assert.AreEqual(TestInterfaceMonobehaviour.MESSAGE, message);

            var copy = Object.Instantiate(testObject);

            Assert.IsNotNull(copy.FirstField);

            message = copy.FirstField.GetMessage();

            Assert.AreEqual(TestInterfaceMonobehaviour.MESSAGE, message);

            Object.DestroyImmediate(copy.gameObject);
        }

        [UnityTest]
        public IEnumerator SetRawValue()
        {
            var rawValue = new TestInterfaceRaw();
            testObject.FirstField = rawValue;

            Assert.IsNotNull(testObject.FirstField);

            yield return null;

            Assert.IsNotNull(testObject.FirstField);

            var message = testObject.FirstField.GetMessage();

            Assert.AreEqual(TestInterfaceRaw.MESSAGE, message);

            var copy = Object.Instantiate(testObject);

            Assert.IsNotNull(copy.FirstField);

            message = copy.FirstField.GetMessage();

            Assert.AreEqual(TestInterfaceRaw.MESSAGE, message);

            Object.DestroyImmediate(copy.gameObject);
        }

        [UnityTest]
        public IEnumerator SetStructValue()
        {
            const string TARGET_MESSAGE = "Struct message!";
            var structValue = new TestInterfaceStruct { Message = TARGET_MESSAGE };

            testObject.FirstField = structValue;

            Assert.IsNotNull(testObject.FirstField);

            yield return null;

            Assert.IsNotNull(testObject.FirstField);

            var message = testObject.FirstField.GetMessage();

            Assert.AreEqual(TARGET_MESSAGE, message);

            var copy = Object.Instantiate(testObject);

            Assert.IsNotNull(copy.FirstField);

            message = copy.FirstField.GetMessage();

            Assert.AreEqual(TARGET_MESSAGE, message);

            Object.DestroyImmediate(copy.gameObject);
        }

        [UnityTest]
        public IEnumerator SetNestedValue()
        {
            const string STRUCT_MESSAGE = "Struct message!";

            var rawValue = new TestInterfaceRaw();
            testObject.FirstField = rawValue;

            var structValue = new TestInterfaceStruct() { Message = STRUCT_MESSAGE };
            rawValue.NestedField = structValue;

            Assert.IsNotNull(testObject.FirstField);
            Assert.IsNotNull(rawValue.NestedField);

            var message = testObject.FirstField.GetMessage();

            Assert.AreEqual(STRUCT_MESSAGE, message);

            yield return null;

            message = testObject.FirstField.GetMessage();

            Assert.AreEqual(STRUCT_MESSAGE, message);
        }

        [UnityTest]
        public IEnumerator InstantiateNestedMonobehaviour()
        {
            var gameObject = new GameObject("Monobehaviour Interface Value");
            gameObject.transform.SetParent(testObject.transform);

            var monoValue = gameObject.AddComponent<TestInterfaceMonobehaviour>();
            var rawValue = new TestInterfaceRaw { NestedField = monoValue };

            testObject.FirstField = rawValue;

            Assert.IsNotNull(testObject.FirstField);

            yield return null;

            Assert.IsNotNull(testObject.FirstField);

            var message = testObject.FirstField.GetMessage();

            Assert.AreEqual(TestInterfaceMonobehaviour.MESSAGE, message);

            var copy = Object.Instantiate(testObject);

            Assert.IsNotNull(copy.FirstField);

            message = copy.FirstField.GetMessage();

            Assert.AreEqual(TestInterfaceMonobehaviour.MESSAGE, message);

            Object.DestroyImmediate(copy.gameObject);
        }
    }
}
