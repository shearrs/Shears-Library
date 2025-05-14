using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace InternProject.Logging.Tests
{
    public class KBLoggerTests
    {
        [Test]
        public void LoggerSingletonInstantiationTest()
        {
            KBLogger.Log("Instantiate");

            Assert.IsTrue(KBLogger.IsInstanceActive());
        }

        [Test]
        public void SetLoggingTypeTest()
        {
            KBLogger.LoggingType = KBLogger.LogType.UnityConsole;

            Assert.AreEqual(KBLogger.LoggingType, KBLogger.LogType.UnityConsole);
        }

        [TearDown]
        public void TearDown()
        {
            var loggers = Object.FindObjectsByType<KBLogger>(FindObjectsSortMode.None);

            for (int i = 0; i < loggers.Length; i++)
                Object.Destroy(loggers[i]);
        }
    }
}
