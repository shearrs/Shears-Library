using NUnit.Framework;
using UnityEngine;

namespace InternProject.Logging.Tests
{
    public class UIConsoleTests : LoggerTestBase
    {
        protected override bool IncludeFancyText => true;
        protected override bool TestClearing => true;

        [SetUp]
        public void SetUp()
        {
            KBLogger.LoggingType = KBLogger.LogType.UIConsole;
            KBLogger.Log("Create UIConsole");
        }

        [Test]
        public void CreationTest()
        {
            LogFancyText("------------CREATION TEST------------\n");
            KBLogger.Log("Create log");

            var uiConsole = GetUIConsole();

            Assert.IsNotNull(uiConsole);

            LogFancyText("\n");
        }  

        protected override string GetMostRecentMessage(string targetMessage)
        {
            string text = GetUIConsole().Text;

            if (text.Length > 0 && targetMessage.Length > 0)
                return text[^targetMessage.Length..];
            else
                return text;
        }

        private KBUIConsoleLogger GetUIConsole()
        {
            return Object.FindAnyObjectByType<KBUIConsoleLogger>();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            var consoles = Object.FindObjectsByType<KBUIConsoleLogger>(FindObjectsSortMode.None);

            for (int i = 0; i < consoles.Length; i++)
                Object.Destroy(consoles[i]);
        }
    }
}
