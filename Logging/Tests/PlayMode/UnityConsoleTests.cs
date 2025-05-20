using NUnit.Framework;
using UnityEngine;

namespace Shears.Logging.Tests
{
    public class UnityConsoleTests : LoggerTestBase
    {
        private string receivedMessage;

        protected override bool IncludeFancyText => false;
        protected override bool TestClearing => false;

        [SetUp]
        public void SetUp()
        {
            SHLogger.LoggingType = SHLogger.LogType.UnityConsole;

            Application.logMessageReceived += ReceiveLogMessage;
        }

        protected override string GetMostRecentMessage(string targetMessage)
        {
            return receivedMessage;
        }

        private void ReceiveLogMessage(string condition, string stackTrace, LogType type)
        => receivedMessage = condition;

        [TearDown]
        public void TearDown()
        {
            Application.logMessageReceived -= ReceiveLogMessage;
        }
    }
}
