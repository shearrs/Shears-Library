using NUnit.Framework;
using UnityEngine;

namespace InternProject.Logging.Tests
{
    public class UnityConsoleTests : LoggerTestBase
    {
        private string receivedMessage;

        protected override bool IncludeFancyText => false;
        protected override bool TestClearing => false;

        [SetUp]
        public void SetUp()
        {
            KBLogger.LoggingType = KBLogger.LogType.UnityConsole;

            Application.logMessageReceived += ReceiveLogMessage;
        }

        protected override string GetMostRecentMessage(string targetMessage)
        {
            return receivedMessage;
        }

        private void ReceiveLogMessage(string condition, string stackTrace, LogType type)
        => receivedMessage = condition;

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            Application.logMessageReceived -= ReceiveLogMessage;
        }
    }
}
