using NUnit.Framework;
using UnityEngine;

namespace Shears.Logging.Tests
{
    public class SHLoggerTests
    {
        [Test]
        public void SetLoggingTypeTest()
        {
            SHLogger.LoggingType = SHLogger.LogType.UnityConsole;

            Assert.AreEqual(SHLogger.LoggingType, SHLogger.LogType.UnityConsole);
        }
    }
}
