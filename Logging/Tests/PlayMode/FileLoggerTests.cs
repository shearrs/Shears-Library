using System.IO;
using NUnit.Framework;

namespace InternProject.Logging.Tests
{
    public class FileLoggerTests : LoggerTestBase
    {
        protected override bool IncludeFancyText => true;
        protected override bool TestClearing => false;

        [SetUp]
        public void SetUp()
        {
            KBLogger.LoggingType = KBLogger.LogType.File;
        }

        [Test]
        public void FileCreationTest()
        {
            LogFancyText("------------CREATION TEST------------\n");
            KBLogger.Log("Create log");

            string filePath = KBLogger.LogFilePath;
            Assert.IsTrue(Directory.Exists(Path.GetDirectoryName(filePath)));

            LogFancyText("\n");
        }

        [Test, Order(100)]
        public void SaveTest()
        {
            KBFormatterTest();
            PrefixTest();
            LogLevelTest();

            KBLogger.SaveLogFile();
        }

        protected override string GetMostRecentMessage(string targetMessage)
        {
            string text;

#if UNITY_WEBGL
            byte[] textBytes = File.ReadAllBytes(KBLogger.LogFilePath);
            text = System.Text.Encoding.UTF8.GetString(textBytes);
#else
            text = File.ReadAllText(KBLogger.LogFilePath);
#endif

            if (text.Length > 0 && targetMessage.Length > 0)
                return text[^targetMessage.Length..];
            else
                return text;
        }
    }
}
