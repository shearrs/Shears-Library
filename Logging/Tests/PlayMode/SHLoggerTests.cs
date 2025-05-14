using NUnit.Framework;
using UnityEngine;

namespace Shears.Logging.Tests
{
    public class SHLoggerTests
    {
        [Test]
        public void LoggerSingletonInstantiationTest()
        {
            SHLogger.Log("Instantiate");

            Assert.IsTrue(SHLogger.IsInstanceActive());
        }

        [Test]
        public void SetLoggingTypeTest()
        {
            SHLogger.LoggingType = SHLogger.LogType.UnityConsole;

            Assert.AreEqual(SHLogger.LoggingType, SHLogger.LogType.UnityConsole);
        }

        [TearDown]
        public void TearDown()
        {
            var loggers = Object.FindObjectsByType<SHLogger>(FindObjectsSortMode.None);

            for (int i = 0; i < loggers.Length; i++)
                Object.Destroy(loggers[i]);
        }
    }
}
