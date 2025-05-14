using NUnit.Framework;
using UnityEngine;

namespace Shears.Logging.Tests
{
    public class UIConsoleTests : LoggerTestBase
    {
        protected override bool IncludeFancyText => true;
        protected override bool TestClearing => true;

        [SetUp]
        public void SetUp()
        {
            SHLogger.LoggingType = SHLogger.LogType.UIConsole;
            SHLogger.Log("Create UIConsole");
        }

        [Test]
        public void CreationTest()
        {
            LogFancyText("------------CREATION TEST------------\n");
            SHLogger.Log("Create log");

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

        private SHUIConsoleLogger GetUIConsole()
        {
            return Object.FindAnyObjectByType<SHUIConsoleLogger>();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            var consoles = Object.FindObjectsByType<SHUIConsoleLogger>(FindObjectsSortMode.None);

            for (int i = 0; i < consoles.Length; i++)
                Object.Destroy(consoles[i]);
        }
    }
}
